﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeMapper.Configuration;
using TypeMapper.Mappers;
using TypeMapper.MappingConventions;
using System.Collections;

namespace TypeMapper.Tests
{
    [TestClass]
    public class ReferenceTypeTests
    {
        private class OuterType
        {
            public InnerType NullInnerType { get; set; }
            public InnerType InnerType { get; set; }
            public List<int> PrimitiveList { get; set; }
            public List<InnerType> ComplexList { get; set; }

            public string String { get; set; }
        }

        private class OuterTypeDto
        {
            public InnerTypeDto NullInnerTypeDto { get; set; }
            public InnerTypeDto InnerTypeDto { get; set; }

            public OuterTypeDto()
            {
                this.NullInnerTypeDto = new InnerTypeDto();
                this.InnerTypeDto = new InnerTypeDto();
            }
        }

        private class InnerType
        {
            public string A { get; set; }
            public string B { get; set; }

            //public OuterType C { get; set; }
        }

        private class InnerTypeDto
        {
            public string A { get; set; }
            public string B { get; set; }

            public OuterType C { get; set; }
        }

        private class AllObjects
        {
            public object String { get; set; }
            public object OuterType { get; set; }
        }

        [TestMethod]
        public void ReferenceSimpleTest()
        {
            var innerType = new InnerType() { A = "this is a test" };

            var source = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType },
                String = "ok"
            };

            //source.InnerType.C = source;

            var target = new OuterType();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );
            Assert.IsTrue( isResultOk );
        }

        [TestMethod]
        public void UseTargetInstanceIfNotNull()
        {
            var innerType = new InnerType() { A = "fadadfsadsffsd" };

            var source = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 0, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType }
            };

            var target = new OuterType()
            {
                InnerType = new InnerType(),
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList(),
                ComplexList = new List<InnerType>() { innerType }
            };

            var beforeMapInnerType = target.InnerType;
            var beforeMapPrimitiveList = target.PrimitiveList;
            var beforeMapComplexList = target.ComplexList;

            var typeMapper = new TypeMapper<CustomMappingConvention>( cfg =>
            {
                cfg.GlobalConfiguration.ReferenceMappingStrategy =
                    ReferenceMappingStrategies.USE_TARGET_INSTANCE_IF_NOT_NULL;
            } );

            typeMapper.Map( source, target );

            Assert.IsTrue( Object.ReferenceEquals( target.InnerType, beforeMapInnerType ) );
            Assert.IsTrue( Object.ReferenceEquals( target.PrimitiveList, beforeMapPrimitiveList ) );
            Assert.IsTrue( Object.ReferenceEquals( target.ComplexList, beforeMapComplexList ) );
        }

        [TestMethod]
        public void CreateNewInstance()
        {
            var innerType = new InnerType() { A = "fadadfsadsffsd" };

            var source = new OuterType()
            {
                PrimitiveList = Enumerable.Range( 0, 10 ).ToList(),

                InnerType = new InnerType()
                {
                    A = "a",
                    B = "b"
                },

                ComplexList = new List<InnerType>()
                {
                    new InnerType() { A = "a", B = "b", },
                    new InnerType() { A = "c", B = "d", }
                }
            };

            var target = new OuterType()
            {
                InnerType = innerType,
                PrimitiveList = Enumerable.Range( 20, 10 ).ToList()
            };

            var primitiveList = target.PrimitiveList;

            var typeMapper = new TypeMapper<CustomMappingConvention>( cfg =>
            {
                cfg.GlobalConfiguration.ReferenceMappingStrategy =
                    ReferenceMappingStrategies.CREATE_NEW_INSTANCE;
            } );

            typeMapper.Map( source, target );
            Assert.IsFalse( Object.ReferenceEquals( target.InnerType, innerType ) );
            Assert.IsFalse( Object.ReferenceEquals( target.PrimitiveList, primitiveList ) );
        }

        [TestMethod]
        public void ObjectToObject()
        {
            throw new Exception( "plain wrong" );

            object source = new AllObjects() { String = "prova", OuterType = new OuterType() { String = "prova" } };
            object target = new AllObjects();

            var typeMapper = new TypeMapper();
            typeMapper.Map( source, target );

            bool isResultOk = typeMapper.VerifyMapperResult( source, target );

            Assert.IsTrue( isResultOk );
            Assert.IsFalse( Object.ReferenceEquals( source, target ) );
            Assert.IsFalse( Object.ReferenceEquals( ((AllObjects)source).OuterType, ((AllObjects)target).OuterType ) );
        }
    }
}
