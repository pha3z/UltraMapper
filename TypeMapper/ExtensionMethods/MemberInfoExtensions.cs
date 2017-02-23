﻿using System;
using System.Reflection;

namespace TypeMapper
{
    public static class MemberInfoExtensions
    {
        public static Type GetMemberType( this MemberInfo memberInfo )
        {
            var field = (memberInfo as FieldInfo);
            if( field != null ) return field.FieldType;

            var property = (memberInfo as PropertyInfo);
            if( property != null ) return property.PropertyType;

            var method = (memberInfo as MethodInfo);
            if( method != null )
            {
                bool isGetterMethod = method.ReturnType != typeof( void ) &&
                    method.GetParameters().Length == 0;

                if( isGetterMethod )
                    return method.ReturnType;

                bool isSetterMethod = method.ReturnType == typeof( void ) &&
                    method.GetParameters().Length == 1;

                if( isSetterMethod )
                    return method.GetParameters()[ 0 ].ParameterType;

                throw new ArgumentException( "Only methods in the form of (T)Get_Value() or (void)Set_Value(T value) are supported." );
            }

            throw new ArgumentException( $"'{memberInfo}' is not supported." );
        }

        public static object GetValue( this MemberInfo memberInfo, object source )
        {
            var field = (memberInfo as FieldInfo);
            if( field != null ) return field.GetValue( source );

            //Property indexes are not supported
            var property = (memberInfo as PropertyInfo);
            if( property != null ) return property.GetValue( source );

            //Only parameterless methods are supported
            var method = (memberInfo as MethodInfo);
            if( method != null ) return method.Invoke( source, null );

            throw new ArgumentException( $"'{memberInfo}' is not supported." );
        }
    }
}