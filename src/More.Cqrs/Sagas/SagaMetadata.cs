// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

namespace More.Domain.Sagas
{
    using Commands;
    using Events;
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Text;
    using static System.Reflection.BindingFlags;

    /// <summary>
    /// Represents the metadata for a saga.
    /// </summary>
    public class SagaMetadata
    {
        static readonly MethodInfo CorrelateOfT = typeof( SagaMetadata ).GetTypeInfo().GetMethod( nameof( Correlate ), Static | NonPublic );
        readonly KeyedCollection<string, SagaMessage> associatedMessages = new KeyedCollection<string, SagaMessage>( m => m.MessageType.FullName );
        readonly KeyedCollection<string, SagaSearchMethod> searchMethods = new KeyedCollection<string, SagaSearchMethod>( m => m.MessageType.FullName );

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaMetadata"/> class.
        /// </summary>
        /// <param name="sagaType">The type of saga.</param>
        /// <param name="sagaDataType">The type of saga type.</param>
        /// <param name="correlationProperty">The <see cref="CorrelationProperty">property</see> the saga is correlated on.</param>
        /// <param name="messages">A <see cref="IReadOnlyCollection{T}">read-only collection</see> of
        /// <see cref="SagaMessage">messages</see> associated with the saga.</param>
        /// <param name="searchMethods">A <see cref="IReadOnlyCollection{T}">read-only collection</see> of
        /// <see cref="SagaSearchMethod">search methods</see> used to find the saga.</param>
        public SagaMetadata(
            Type sagaType,
            Type sagaDataType,
            PropertyInfo correlationProperty,
            IReadOnlyCollection<SagaMessage> messages,
            IReadOnlyCollection<SagaSearchMethod> searchMethods )
        {
            Arg.NotNull( sagaType, nameof( sagaType ) );
            Arg.NotNull( sagaDataType, nameof( sagaDataType ) );
            Arg.NotNull( messages, nameof( messages ) );
            Arg.NotNull( correlationProperty, nameof( correlationProperty ) );
            Arg.NotNull( searchMethods, nameof( searchMethods ) );

            SagaType = sagaType;
            SagaDataType = sagaDataType;
            CorrelationProperty = correlationProperty;
            VerifyCorrelatedPropertyTypeIsAllowed( sagaType, CorrelationProperty );

            var hasAtLeastOneStartMessage = false;

            foreach ( var message in messages )
            {
                hasAtLeastOneStartMessage |= message.StartsSaga;
                associatedMessages.Add( message );
            }

            if ( !hasAtLeastOneStartMessage )
            {
                throw new SagaConfigurationException(
                    SR.MissingSagaStartMessage.FormatDefault(
                        typeof( IStartWith<> ).Name,
                        typeof( IStartWhen<> ).Name,
                        sagaType.Name ) );
            }

            foreach ( var searchMethod in searchMethods )
            {
                this.searchMethods.Add( searchMethod );
            }
        }

        /// <summary>
        /// Gets the type of saga.
        /// </summary>
        /// <value>The saga type.</value>
        public Type SagaType { get; }

        /// <summary>
        /// Gets the type of saga type.
        /// </summary>
        /// <value>The saga data type.</value>
        public Type SagaDataType { get; }

        /// <summary>
        /// Gets a read-only collection of associated messages.
        /// </summary>
        /// <value>A <see cref="IReadOnlyCollection{T}">read-only collection</see> of associated <see cref="SagaMessage">messages</see>.</value>
        public IReadOnlyCollection<SagaMessage> AssociatedMessages => associatedMessages;

        /// <summary>
        /// Gets a read-only collection of search methods used to find the saga.
        /// </summary>
        /// <value>A <see cref="IReadOnlyCollection{T}">read-only collection</see> of <see cref="SagaSearchMethod">search methods</see>.</value>
        public IReadOnlyCollection<SagaSearchMethod> SearchMethods => searchMethods;

        /// <summary>
        /// Gets the property the saga is correlated with.
        /// </summary>
        /// <value>The saga correlation <see cref="PropertyInfo">property</see>.</value>
        public PropertyInfo CorrelationProperty { get; }

        /// <summary>
        /// Determines whether the saga can be started by the specified message type.
        /// </summary>
        /// <param name="messageType">The qualified type name of the message to evaluate.</param>
        /// <returns>True if the saga can be started by the specifed message type; otherwise, false.</returns>
        public bool CanStartSaga( string messageType )
        {
            if ( associatedMessages.TryGetValue( messageType, out var message ) )
            {
                return message.StartsSaga;
            }

            return false;
        }

        /// <summary>
        /// Creates the metadata for a saga from the specified type.
        /// </summary>
        /// <param name="sagaType">The type of saga to create the metadata for.</param>
        /// <returns>A new <see cref="SagaMetadata"/> instance.</returns>
        /// <exception cref="ArgumentException"><paramref name="sagaType"/> is not a <see cref="ISaga{TData}"/>.</exception>
        public static SagaMetadata Create( Type sagaType )
        {
            if ( !sagaType.IsSaga() )
            {
                throw new ArgumentException( SR.InvalidSaga.FormatDefault( sagaType.Name ) );
            }

            var sagaDataType = sagaType.GetSagaDataType();
            var method = CorrelateOfT.MakeGenericMethod( sagaDataType );
            var correlate = (Action<Type, ICorrelateSagaToMessage>) method.CreateDelegate( typeof( Action<Type, ICorrelateSagaToMessage> ) );
            var correlator = new SagaCorrelator();

            correlate( sagaType, correlator );

            var associatedMessages = GetAssociatedMessages( sagaType ).ToArray();
            var mapping = EnsureSinglePropertyMapping( correlator, associatedMessages, sagaType );
            var correlationProperty = mapping.SagaDataProperty;
            var searchMethods = new List<SagaSearchMethod>();

            SetSearchMethodForMappings( correlator, sagaType, sagaDataType, associatedMessages, searchMethods );
            EnsureMappingToStartMessage( correlator, sagaType, associatedMessages );

            return new SagaMetadata( sagaType, sagaDataType, correlationProperty, associatedMessages, searchMethods );
        }

        /// <summary>
        /// Attempts to resolve the saga search method for the specified message type.
        /// </summary>
        /// <param name="messageType">The qualified name of the message type to retrieve the search method for.</param>
        /// <param name="searchMethod">The resolved <see cref="SagaSearchMethod">search method</see> or <c>null</c> of no search method is matched.</param>
        /// <returns>True if the <paramref name="searchMethod">search method</paramref> is matched; otherwise, false.</returns>
        public bool TryGetSearchMethod( string messageType, out SagaSearchMethod searchMethod ) =>
            searchMethods.TryGetValue( messageType, out searchMethod );

        static void Correlate<TData>( Type sagaType, ICorrelateSagaToMessage correlation ) where TData : class, ISagaData =>
            ( (ISaga<TData>) FormatterServices.GetUninitializedObject( sagaType ) ).CorrelateUsing( correlation );

        static SagaToMessageMap EnsureSinglePropertyMapping( SagaCorrelator correlator, IEnumerable<SagaMessage> associatedMessages, Type sagaType )
        {
            Contract.Requires( correlator != null );
            Contract.Requires( associatedMessages != null );
            Contract.Requires( sagaType != null );
            Contract.Ensures( Contract.Result<SagaToMessageMap>() != null );

            var propertyMappings = ( from message in associatedMessages
                                     where message.StartsSaga
                                     from mapping in correlator.Mappings
                                     where mapping.MessageType == message.MessageType
                                     group mapping by mapping.SagaDataProperty.Name ).ToArray();

            switch ( propertyMappings.Length )
            {
                case 0:
                    throw new SagaConfigurationException( SR.SagaNotCorrelated );
                case 1:
                    return propertyMappings[0].First();
                default:
                    var messageProperties = new StringBuilder();
                    var names = from mappings in propertyMappings
                                from mapping in mappings
                                select mapping.MessageProperty.Name;

                    using ( var name = names.Distinct().Reverse().GetEnumerator() )
                    {
                        name.MoveNext();
                        messageProperties.Append( SR.AndConjunction );
                        messageProperties.Append( name.Current );

                        while ( name.MoveNext() )
                        {
                            messageProperties.Insert( 0, ", " );
                            messageProperties.Insert( 0, name.Current );
                        }
                    }

                    throw new SagaConfigurationException( SR.TooManySagaCorrelationProperties.FormatDefault( sagaType.Name, messageProperties ) );
            }
        }

        static void EnsureMappingToStartMessage( SagaCorrelator correlator, Type sagaType, IEnumerable<SagaMessage> associatedMessages )
        {
            Contract.Requires( correlator != null );
            Contract.Requires( sagaType != null );
            Contract.Requires( associatedMessages != null );

            foreach ( var message in associatedMessages.Where( message => message.StartsSaga ) )
            {
                var messageType = message.MessageType.GetTypeInfo();
                var matches = from mapping in correlator.Mappings
                              where mapping.MessageType.GetTypeInfo().IsAssignableFrom( messageType )
                              select mapping;

                if ( !matches.Any() )
                {
                    throw new SagaConfigurationException( SR.UnmappedSagaStartMessage.FormatDefault( message.MessageType.Name, sagaType.Name ) );
                }
            }
        }

        static void SetSearchMethodForMappings(
            SagaCorrelator correlator,
            Type sagaType,
            Type sagaDataType,
            IEnumerable<SagaMessage> associatedMessages,
            ICollection<SagaSearchMethod> searchMethods )
        {
            Contract.Requires( correlator != null );
            Contract.Requires( sagaType != null );
            Contract.Requires( sagaDataType != null );
            Contract.Requires( associatedMessages != null );
            Contract.Requires( searchMethods != null );

            foreach ( var mapping in correlator.Mappings )
            {
                var messageType = mapping.MessageType.GetTypeInfo();
                var associatedMessage = associatedMessages.FirstOrDefault( message => messageType.IsAssignableFrom( message.MessageType.GetTypeInfo() ) );

                if ( associatedMessage == null )
                {
                    var interfaces = new[] { typeof( IStartWith<> ), typeof( IStartWhen<> ), typeof( IReceiveEvent<> ), typeof( IHandleCommand<> ) };
                    var interfaceNames = new StringBuilder();

                    using ( var @interface = interfaces.Reverse().GetEnumerator() )
                    {
                        @interface.MoveNext();
                        interfaceNames.Append( SR.OrConjunction );
                        interfaceNames.Append( @interface.Current.Name );

                        while ( @interface.MoveNext() )
                        {
                            interfaceNames.Insert( 0, ", " );
                            interfaceNames.Insert( 0, @interface.Current.Name );
                        }
                    }

                    throw new SagaConfigurationException( SR.UnhandledSagaMessage.FormatDefault( sagaType.Name, messageType.Name, interfaceNames ) );
                }

                SetSearchMethod( mapping, sagaDataType, searchMethods );
            }
        }

        static void SetSearchMethod( SagaToMessageMap mapping, Type sagaDataType, ICollection<SagaSearchMethod> searchMethods )
        {
            Contract.Requires( mapping != null );
            Contract.Requires( sagaDataType != null );
            Contract.Requires( searchMethods != null );

            var searchMethodType = typeof( SearchForSagaByProperty<> ).MakeGenericType( sagaDataType );
            var searchMethod = new ByPropertySagaSearchMethod(
                                    searchMethodType,
                                    mapping.MessageType,
                                    mapping.SagaDataProperty.Name,
                                    mapping.ReadMessageProperty );

            searchMethods.Add( searchMethod );
        }

        static IEnumerable<SagaMessage> GetAssociatedMessages( Type sagaType )
        {
            Contract.Requires( sagaType != null );
            Contract.Ensures( Contract.Result<IEnumerable<SagaMessage>>() != null );

            var interfaces = sagaType.GetTypeInfo().GetInterfaces();
            var messages = new HashSet<SagaMessage>(
                                 GetFilteredMessagesForSaga( interfaces, typeof( IStartWith<> ), typeof( IStartWhen<> ) )
                                .Select( messageType => new SagaMessage( messageType, startsSaga: true ) ),
                                 SagaMessageComparer.Instance );

            foreach ( var messageType in GetFilteredMessagesForSaga( interfaces, typeof( IHandleCommand<> ), typeof( IReceiveEvent<> ), typeof( ITimeoutWhen<> ) ) )
            {
                messages.Add( new SagaMessage( messageType, startsSaga: false ) );
            }

            return messages;
        }

        static IEnumerable<Type> GetFilteredMessagesForSaga( Type[] interfaces, params Type[] filters )
        {
            Contract.Requires( interfaces != null );
            Contract.Requires( filters != null );
            Contract.Ensures( Contract.Result<IEnumerable<Type>>() != null );

            foreach ( var @interface in interfaces.Select( i => i.GetTypeInfo() ).Where( i => i.IsGenericType ) )
            {
                var interfaceDefinition = @interface.GetGenericTypeDefinition();

                if ( filters.Any( filter => filter.Equals( interfaceDefinition ) ) )
                {
                    yield return @interface.GenericTypeArguments[0];
                }
            }
        }

        static void VerifyCorrelatedPropertyTypeIsAllowed( Type sagaType, PropertyInfo property )
        {
            Contract.Requires( sagaType != null );
            Contract.Requires( property != null );

            var type = property.PropertyType;
            var typeCode = Type.GetTypeCode( type );
            var evaluate = true;

            while ( evaluate )
            {
                switch ( typeCode )
                {
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                    case TypeCode.UInt16:
                    case TypeCode.UInt32:
                    case TypeCode.UInt64:
                    case TypeCode.String:
                        return;
                    case TypeCode.Object:
                        if ( type == typeof( Guid ) )
                        {
                            return;
                        }

                        if ( type.IsNullable() )
                        {
                            type = type.GenericTypeArguments[0];
                            typeCode = Type.GetTypeCode( type );
                        }
                        else
                        {
                            evaluate = false;
                        }

                        break;
                }
            }

            throw new SagaConfigurationException( SR.UnsupportedCorrelationValueType.FormatDefault( type.Name, property.Name, sagaType.Name ) );
        }
    }
}