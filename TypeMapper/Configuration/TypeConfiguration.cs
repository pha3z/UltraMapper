﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;
using TypeMapper.Internals;
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;

namespace TypeMapper.Configuration
{
    public class TypeConfiguration<T> : TypeConfiguration where T : IMappingConvention, new()
    {
        public TypeConfiguration() { }

        public TypeConfiguration( Action<T> config )
              : base( new T() ) { config?.Invoke( (T)this.MappingConvention ); }
    }

    public class TypeConfiguration
    {
        private Dictionary<TypePair, PropertyConfiguration> _typeMappings =
            new Dictionary<TypePair, PropertyConfiguration>();

        public IMappingConvention MappingConvention { get; protected set; }
        public ObjectMapperConfiguration ObjectMappers { get; set; }

        public TypeConfiguration()
        {
            this.MappingConvention = new DefaultMappingConvention();
            this.ObjectMappers = new ObjectMapperConfiguration()
                .Add<BuiltInTypeMapper>()
                .Add<ReferenceMapper>();
                //.Add<DictionaryMapper>() //since dictionaries are collections, to be correctly handled must be evaluated by a suitable mapper before a CollectionMapper
                //.Add<CollectionMapper>();
        }

        public TypeConfiguration( Action<DefaultMappingConvention> config )
            : this()
        {
            config?.Invoke( (DefaultMappingConvention)this.MappingConvention );
        }

        public TypeConfiguration( IMappingConvention mappingConvention )
        {
            this.MappingConvention = mappingConvention;
        }

        public PropertyConfiguration<TSource, TTarget> MapTypes<TSource, TTarget>( TSource source, TTarget target )
        {
            return MapTypes<TSource, TTarget>();
        }

        public PropertyConfiguration<TSource, TTarget> MapTypes<TSource, TTarget>()
        {
            var map = this.MapTypes( typeof( TSource ), typeof( TTarget ) );
            return new PropertyConfiguration<TSource, TTarget>( map, this.ObjectMappers );
        }

        public PropertyConfiguration MapTypes( Type source, Type target )
        {
            var typePair = new TypePair( source, target );

            PropertyConfiguration typeMapping;
            if( _typeMappings.TryGetValue( typePair, out typeMapping ) )
                return typeMapping;

            var propertymappings = new PropertyConfiguration( source, target, this.MappingConvention, this.ObjectMappers );
            _typeMappings.Add( typePair, propertymappings );

            return propertymappings;
        }

        /// <summary>
        /// Gets the property mapping associated with the source/target type pair.
        /// If the mapping for the pair does not exist, it is created.
        /// </summary>
        /// <param name="key">The source/target type pair</param>
        /// <returns>The mapping associated with the type pair</returns>
        public PropertyConfiguration this[ Type sourceType, Type targetType ]
        {
            get
            {
                var typePair = new TypePair( sourceType, targetType );
                return this[ typePair ];
            }
        }

        /// <summary>
        /// Gets the property mapping associated with the source/target type pair.
        /// If the mapping for the pair does not exist, it is created.
        /// </summary>
        /// <param name="key">The source/target type pair</param>
        /// <returns>The mapping associated with the type pair</returns>
        internal PropertyConfiguration this[ TypePair key ]
        {
            get
            {
                PropertyConfiguration typeMapping = null;
                if( !_typeMappings.TryGetValue( key, out typeMapping ) )
                    typeMapping = this.MapTypes( key.SourceType, key.TargetType );

                return typeMapping;
            }
        }
    }
}