﻿namespace Caxivitual.Lunacub.Importing;

/// <summary>
/// Specifies the resource releasing result.
/// </summary>
public enum ReleaseStatus {
    /// <summary>
    /// Resource object unregistered from cache and got disposed.
    /// </summary>
    Success = 0,
    
    /// <summary>
    /// Resource object unregistered from cache but not disposed, indicates a leak.
    /// </summary>
    NotDisposed,
    
    /// <summary>
    /// Resource is being imported but received cancellation signal.
    /// </summary>
    Canceled,
    
    /// <summary>
    /// The specified resource object or resource handle to release is or represents null.
    /// </summary>
    Null = -1,
    
    /// <summary>
    /// The specified resource object is not a resource object of the <see cref="ImportEnvironment"/> instance.
    /// </summary>
    InvalidResource = -2,
    
    /// <summary>
    /// The resource object of <see cref="ResourceHandle"/> is a valid resource object, but the associated Id is unmatched.
    /// </summary>
    IdIncompatible = -3,
    
    /// <summary>
    /// The resource with the associated Id hasn't been imported yet.
    /// </summary>
    NotImported = -4,
    
    /// <summary>
    /// Unspecified error happened.
    /// </summary>
    Unspecified = int.MinValue,
}