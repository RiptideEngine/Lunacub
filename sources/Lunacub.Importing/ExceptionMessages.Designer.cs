﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Caxivitual.Lunacub.Importing {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class ExceptionMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ExceptionMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Caxivitual.Lunacub.Importing.ExceptionMessages", typeof(ExceptionMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Key cannot be empty or consist of only whitespace characters..
        /// </summary>
        internal static string EmptyOrWhitespaceKey {
            get {
                return ResourceManager.GetString("EmptyOrWhitespaceKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Null Stream was returned for resource with Id of {0}..
        /// </summary>
        internal static string NullResourceStream {
            get {
                return ResourceManager.GetString("NullResourceStream", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot find chunk {0} in compiled resource binary..
        /// </summary>
        internal static string ResourceMissingChunk {
            get {
                return ResourceManager.GetString("ResourceMissingChunk", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resource Stream for resource with ID of &apos;{0}&apos; must be readable and seekable..
        /// </summary>
        internal static string ResourceStreamMustBeSeekableOrReadable {
            get {
                return ResourceManager.GetString("ResourceStreamMustBeSeekableOrReadable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resource Stream for resource with ID of &apos;{0}&apos; must not be writable..
        /// </summary>
        internal static string ResourceStreamMustNotBeWritable {
            get {
                return ResourceManager.GetString("ResourceStreamMustNotBeWritable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unexpected task status &apos;{0}&apos; for resource id {1}..
        /// </summary>
        internal static string UnexpectedTaskStatus {
            get {
                return ResourceManager.GetString("UnexpectedTaskStatus", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to find Deserializer with key &apos;{0}&apos;..
        /// </summary>
        internal static string UnregisteredDeserializer {
            get {
                return ResourceManager.GetString("UnregisteredDeserializer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resource with Id of {0} is unregistered..
        /// </summary>
        internal static string UnregisteredResourceId {
            get {
                return ResourceManager.GetString("UnregisteredResourceId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Resource with name &apos;{0}&apos; is unregistered..
        /// </summary>
        internal static string UnregisteredResourceName {
            get {
                return ResourceManager.GetString("UnregisteredResourceName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Compiled resource version {0}.{1} is not supported..
        /// </summary>
        internal static string UnsupportedCompiledResourceVersion {
            get {
                return ResourceManager.GetString("UnsupportedCompiledResourceVersion", resourceCulture);
            }
        }
    }
}
