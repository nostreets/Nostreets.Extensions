using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NostreetsExtensions.DataControl.Attributes.PostSharp
{
    #region MulticastAttribute
    //
    // Summary:
    //     Custom attribute that can be applied to multiple elements using wildcards.
    [Serializer(null)]
    public abstract class MulticastAttribute : Attribute
    {
        //
        // Summary:
        //     PostSharp.Extensibility.MulticastAttribute
        protected MulticastAttribute();

        //
        // Summary:
        //     Gets or sets the kind of elements to which this custom attributes applies.
        [AspectSerializerIgnoreAttribute]
        public MulticastTargets AttributeTargetElements { get; set; }
        //
        // Summary:
        //     Gets or sets the assemblies to which the current attribute apply.
        [AspectSerializerIgnoreAttribute]
        public string AttributeTargetAssemblies { get; set; }
        //
        // Summary:
        //     Gets or sets the expression specifying to which types this instance applies.
        [AspectSerializerIgnoreAttribute]
        public string AttributeTargetTypes { get; set; }
        //
        // Summary:
        //     Gets or sets the visibilities of types to which this attribute applies.
        [AspectSerializerIgnoreAttribute]
        public MulticastAttributes AttributeTargetTypeAttributes { get; set; }
        //
        // Summary:
        //     Gets or sets the visibilities of types to which this attribute applies, when
        //     this type is external to the current module.
        [AspectSerializerIgnoreAttribute]
        public MulticastAttributes AttributeTargetExternalTypeAttributes { get; set; }
        //
        // Summary:
        //     Gets or sets the expression specifying to which members this instance applies.
        [AspectSerializerIgnoreAttribute]
        public string AttributeTargetMembers { get; set; }
        //
        // Summary:
        //     Gets or sets the visibilities, scopes, virtualities, and implementation of members
        //     to which this attribute applies.
        [AspectSerializerIgnoreAttribute]
        public MulticastAttributes AttributeTargetMemberAttributes { get; set; }
        //
        // Summary:
        //     Gets or sets the visibilities, scopes, virtualities, and implementation of members
        //     to which this attribute applies, when the member is external to the current module.
        [AspectSerializerIgnoreAttribute]
        public MulticastAttributes AttributeTargetExternalMemberAttributes { get; set; }
        //
        // Summary:
        //     Gets or sets the expression specifying to which parameters this instance applies.
        [AspectSerializerIgnoreAttribute]
        public string AttributeTargetParameters { get; set; }
        //
        // Summary:
        //     Gets or sets the passing style (by value, out or ref) of parameters to which
        //     this attribute applies.
        [AspectSerializerIgnoreAttribute]
        public MulticastAttributes AttributeTargetParameterAttributes { get; set; }
        //
        // Summary:
        //     If true, indicates that this attribute removes all other instances of the same
        //     attribute type from the set of elements defined by the current instance.
        [AspectSerializerIgnoreAttribute]
        public bool AttributeExclude { get; set; }
        //
        // Summary:
        //     Gets or sets the priority of the current attribute in case that multiple instances
        //     are defined on the same element (lower values are processed before).
        [AspectSerializerIgnoreAttribute]
        public int AttributePriority { get; set; }
        //
        // Summary:
        //     Determines whether this attribute replaces other attributes found on the target
        //     declarations.
        [AspectSerializerIgnoreAttribute]
        public bool AttributeReplace { get; set; }
        //
        // Summary:
        //     Determines whether this attribute is inherited
        [AspectSerializerIgnoreAttribute]
        public MulticastInheritance AttributeInheritance { get; set; }
        [AspectSerializerIgnoreAttribute]
        [Obsolete("Do not use this property in customer code.", true)]
        public long AttributeId { get; set; }
    }

    #endregion

    #region MulticastInheritance

    //
    // Summary:
    //     Kind of inheritance of PostSharp.Extensibility.MulticastAttribute.
    public enum MulticastInheritance
    {
        //
        // Summary:
        //     No inheritance.
        None = 0,
        //
        // Summary:
        //     The instance is inherited to children of the original element, but multicasting
        //     is not applied to members of children.
        Strict = 1,
        //
        // Summary:
        //     The instance is inherited to children of the original element and multicasting
        //     is applied to members of children.
        Multicast = 2
    }

    #endregion

    #region MulticastAttributes
    //
    // Summary:
    //     Attributes of elements to which multicast custom attributes (PostSharp.Extensibility.MulticastAttribute)
    //     apply.
    [Flags]
    public enum MulticastAttributes
    {
        //
        // Summary:
        //     Specifies that the set of target attributes is inherited from the parent custom
        //     attribute.
        Default = 0,
        //
        // Summary:
        //     Private (visible inside the current type).
        Private = 2,
        //
        // Summary:
        //     Protected (visible inside derived types).
        Protected = 4,
        //
        // Summary:
        //     Internal (visible inside the current assembly).
        Internal = 8,
        //
        // Summary:
        //     Internal and protected (visible inside derived types that are defined in the
        //     current assembly).
        InternalAndProtected = 16,
        //
        // Summary:
        //     Internal or protected (visible inside all derived types and in the current assembly).
        InternalOrProtected = 32,
        //
        // Summary:
        //     Public (visible everywhere).
        Public = 64,
        //
        // Summary:
        //     Any visibility.
        AnyVisibility = 126,
        //
        // Summary:
        //     Static scope.
        Static = 128,
        //
        // Summary:
        //     Instance scope.
        Instance = 256,
        //
        // Summary:
        //     Any scope (PostSharp.Extensibility.MulticastAttributes.Static | PostSharp.Extensibility.MulticastAttributes.Instance).
        AnyScope = 384,
        //
        // Summary:
        //     Abstract methods.
        Abstract = 512,
        //
        // Summary:
        //     Concrete (non-abstract) methods.
        NonAbstract = 1024,
        //
        // Summary:
        //     Any abstraction (PostSharp.Extensibility.MulticastAttributes.Abstract | PostSharp.Extensibility.MulticastAttributes.NonAbstract).
        AnyAbstraction = 1536,
        //
        // Summary:
        //     Virtual methods.
        Virtual = 2048,
        //
        // Summary:
        //     Non-virtual methods.
        NonVirtual = 4096,
        //
        // Summary:
        //     Any virtuality (PostSharp.Extensibility.MulticastAttributes.Virtual | PostSharp.Extensibility.MulticastAttributes.NonVirtual).
        AnyVirtuality = 6144,
        //
        // Summary:
        //     Managed code implementation.
        Managed = 8192,
        //
        // Summary:
        //     Non-managed code implementation (external or system).
        NonManaged = 16384,
        //
        // Summary:
        //     Any implementation (PostSharp.Extensibility.MulticastAttributes.Managed | PostSharp.Extensibility.MulticastAttributes.NonManaged).
        AnyImplementation = 24576,
        //
        // Summary:
        //     Literal fields.
        Literal = 32768,
        //
        // Summary:
        //     Non-literal fields.
        NonLiteral = 65536,
        //
        // Summary:
        //     Any field literality (PostSharp.Extensibility.MulticastAttributes.Literal | PostSharp.Extensibility.MulticastAttributes.NonLiteral).
        AnyLiterality = 98304,
        //
        // Summary:
        //     Input parameters.
        InParameter = 131072,
        //
        // Summary:
        //     Compiler-generated code.
        CompilerGenerated = 262144,
        //
        // Summary:
        //     User-generated code (anything expected PostSharp.Extensibility.MulticastAttributes.CompilerGenerated).
        UserGenerated = 524288,
        //
        // Summary:
        //     Any code generation (PostSharp.Extensibility.MulticastAttributes.CompilerGenerated
        //     | PostSharp.Extensibility.MulticastAttributes.UserGenerated)l
        AnyGeneration = 786432,
        //
        // Summary:
        //     Output (out in C#) parameters.
        OutParameter = 1048576,
        //
        // Summary:
        //     Input/Output (ref in C#) parameters.
        RefParameter = 2097152,
        //
        // Summary:
        //     Any kind of parameter passing (PostSharp.Extensibility.MulticastAttributes.InParameter
        //     | PostSharp.Extensibility.MulticastAttributes.OutParameter | PostSharp.Extensibility.MulticastAttributes.RefParameter).
        AnyParameter = 3276800,
        //
        // Summary:
        //     All members.
        All = 4194302
    }
    #endregion

    #region MulticastTargets
    //
    // Summary:
    //     Kinds of targets to which multicast custom attributes (PostSharp.Extensibility.MulticastAttribute)
    //     can apply.
    [Flags]
    public enum MulticastTargets
    {
        //
        // Summary:
        //     Specifies that the set of target elements is inherited from the parent custom
        //     attribute.
        Default = 0,
        //
        // Summary:
        //     Class.
        Class = 1,
        //
        // Summary:
        //     Structure.
        Struct = 2,
        //
        // Summary:
        //     Enumeration.
        Enum = 4,
        //
        // Summary:
        //     Delegate.
        Delegate = 8,
        //
        // Summary:
        //     Interface.
        Interface = 16,
        //
        // Summary:
        //     Any type (PostSharp.Extensibility.MulticastTargets.Class, PostSharp.Extensibility.MulticastTargets.Struct,
        //     PostSharp.Extensibility.MulticastTargets.Enum, PostSharp.Extensibility.MulticastTargets.Delegate
        //     or PostSharp.Extensibility.MulticastTargets.Interface).
        AnyType = 31,
        //
        // Summary:
        //     Field.
        Field = 32,
        //
        // Summary:
        //     Method (but not constructor).
        Method = 64,
        //
        // Summary:
        //     Instance constructor.
        InstanceConstructor = 128,
        //
        // Summary:
        //     Static constructor.
        StaticConstructor = 256,
        //
        // Summary:
        //     Property (but not methods inside the property).
        Property = 512,
        //
        // Summary:
        //     Event (but not methods inside the event).
        Event = 1024,
        //
        // Summary:
        //     Any member (PostSharp.Extensibility.MulticastTargets.Field, PostSharp.Extensibility.MulticastTargets.Method,
        //     PostSharp.Extensibility.MulticastTargets.InstanceConstructor, PostSharp.Extensibility.MulticastTargets.StaticConstructor,
        //     PostSharp.Extensibility.MulticastTargets.Property, PostSharp.Extensibility.MulticastTargets.Event).
        AnyMember = 2016,
        //
        // Summary:
        //     Assembly.
        Assembly = 2048,
        //
        // Summary:
        //     Method or property parameter.
        Parameter = 4096,
        //
        // Summary:
        //     Method or property return value.
        ReturnValue = 8192,
        //
        // Summary:
        //     All element kinds.
        All = 16383
    }

    #endregion

    #region SerializerAttribute
    /// <summary>
    ///       Custom attribute that, when applied to a type, specifies its serializer for use by the <see cref="T:PostSharp.Serialization.PortableFormatter" />.
    ///       </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class SerializerAttribute : Attribute
    {
        /// <summary>
        ///       Gets the serializer type.
        ///       </summary>
        public Type SerializerType
        {
            get;
            private set;
        }

        /// <summary>
        ///       Initializes a new <see cref="T:PostSharp.Serialization.SerializerAttribute" />.
        ///       </summary>
        /// <param name="serializerType">Serializer type. This type must implement <see cref="T:PostSharp.Serialization.ISerializer" /> or <see cref="T:PostSharp.Serialization.ISerializerFactory" />,
        ///       and must have a public default constructor. If <paramref name="serializerType" /> is a generic type, if must have the same number
        ///       of generic type parameters as the target type, and have a compatible set of constraints.</param>
        public SerializerAttribute(Type serializerType)
        {
            this.SerializerType = serializerType;
        }
    }

    #endregion

    #region AspectSerializerIgnoreAttribute
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal sealed class AspectSerializerIgnoreAttribute : Attribute
    {
        public AspectSerializerIgnoreAttribute()
        {
        }
    }
    #endregion

}
