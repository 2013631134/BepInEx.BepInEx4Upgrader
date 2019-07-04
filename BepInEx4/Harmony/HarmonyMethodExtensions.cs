﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Harmony
{
    public static class HarmonyMethodExtensions
    {
        public static void CopyTo(this HarmonyMethod from, HarmonyMethod to)
        {
            if (to == null) return;
            var fromTrv = Traverse.Create(from);
            var toTrv = Traverse.Create(to);
            HarmonyMethod.HarmonyFields().ForEach(delegate(string f)
            {
                var value = fromTrv.Field(f).GetValue();
                if (value != null) toTrv.Field(f).SetValue(value);
            });
        }

        public static HarmonyMethod Clone(this HarmonyMethod original)
        {
            var harmonyMethod = new HarmonyMethod();
            original.CopyTo(harmonyMethod);
            return harmonyMethod;
        }

        public static HarmonyMethod Merge(this HarmonyMethod master, HarmonyMethod detail)
        {
            if (detail == null) return master;
            var harmonyMethod = new HarmonyMethod();
            var resultTrv = Traverse.Create(harmonyMethod);
            var masterTrv = Traverse.Create(master);
            var detailTrv = Traverse.Create(detail);
            HarmonyMethod.HarmonyFields().ForEach(delegate(string f)
            {
                var value = masterTrv.Field(f).GetValue();
                var value2 = detailTrv.Field(f).GetValue();
                resultTrv.Field(f).SetValue(value2 ?? value);
            });
            return harmonyMethod;
        }

        public static List<HarmonyMethod> GetHarmonyMethods(this Type type)
        {
            return (from HarmonyAttribute attr in
                    from attr in type.GetCustomAttributes(true)
                    where attr is HarmonyAttribute
                    select attr
                select attr.info).ToList();
        }

        public static List<HarmonyMethod> GetHarmonyMethods(this MethodBase method)
        {
            if (method is DynamicMethod) return new List<HarmonyMethod>();
            return (from HarmonyAttribute attr in
                    from attr in method.GetCustomAttributes(true)
                    where attr is HarmonyAttribute
                    select attr
                select attr.info).ToList();
        }
    }
}