﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using WebApiClient.Contexts;

namespace WebApiClient
{
    /// <summary>
    /// 提供参数值和参数的属性值输入合法性验证
    /// </summary>
    static class ParameterValidator
    {
        /// <summary>
        /// 类型的属性否需要验证缓存
        /// </summary>
        private static readonly ConcurrentCache<Type, bool> cache = new ConcurrentCache<Type, bool>();

        /// <summary>
        /// 返回是否需要进行属性验证
        /// </summary>
        /// <param name="instance">实例</param>
        /// <returns></returns>
        private static bool IsNeedValidateProperty(object instance)
        {
            if (instance == null)
            {
                return false;
            }

            var type = instance.GetType();
            if (type == typeof(string) || type.GetTypeInfo().IsValueType == true)
            {
                return false;
            }

            return cache.GetOrAdd(type, t => t.GetProperties().Any(p => p.CanRead && p.IsDefined(typeof(ValidationAttribute), true)));
        }

        /// <summary>
        /// 验证参数值输入合法性
        /// 验证参数的属性值输入合法性
        /// </summary>
        /// <param name="parameter">参数描述</param>
        /// <exception cref="ValidationException"></exception>
        public static void Validate(ApiParameterDescriptor parameter)
        {
            var name = parameter.Name;
            var instance = parameter.Value;

            foreach (var validation in parameter.ValidationAttributes)
            {
                validation.Validate(instance, name);
            }

            if (IsNeedValidateProperty(instance) == true)
            {
                var ctx = new ValidationContext(instance) { MemberName = name };
                Validator.ValidateObject(instance, ctx, true);
            }
        }
    }
}