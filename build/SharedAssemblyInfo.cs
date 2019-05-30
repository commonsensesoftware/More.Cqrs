// Copyright (c) Commonsense Software. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Runtime.InteropServices;

// The WriteCodeFragment task currently only supports attributes with string values.This
// makes it impossible to use this technique to generate[assembly: ComVisible(false)]
// and [assembly: CLSCompliant(true)]. If/when the feature is enhanced, the following
// items can be uncommented and the SharedAssemblyInfo.cs file can be deleted.
//
// REF: https://github.com/Microsoft/msbuild/issues/2281
[assembly: ComVisible( false )]
[assembly: CLSCompliant( true )]