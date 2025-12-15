using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBus
{
    public static class PredefinedAssemblyUtil
    {
        private enum AssemblyType
        {
            AssemblyCSharp,
            AssemblyCSharpEditor,
            AssemblyCSharpEditorFirstPass,
            AssemblyCSharpFirstPass
        }

        private static AssemblyType? GetAssemblyType(string assemblyName)
        {
            return assemblyName switch
            {
                "Assembly-CSharp" => AssemblyType.AssemblyCSharp,
                "Assembly-CSharp-Editor" => AssemblyType.AssemblyCSharpEditor,
                "Assembly-CSharp-Editor-firstpass" => AssemblyType.AssemblyCSharpEditorFirstPass,
                "Assembly-CSharp-firstpass" => AssemblyType.AssemblyCSharpFirstPass,
                _ => null
            };
        }

        public static List<Type> GetTypes(Type interfaceType)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Dictionary<AssemblyType, Type[]> assemblyTypes = new();
            List < Type > types = new();
            foreach (var t in assemblies)
            {
                var assemblyType = GetAssemblyType(t.GetName().Name);
                if (assemblyType != null) assemblyTypes.Add((AssemblyType)assemblyType, t.GetTypes());
            }

            assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharp, out var assemblyCSharpTypes);
            AddTypesFromAssembly(assemblyCSharpTypes, interfaceType, types);

            assemblyTypes.TryGetValue(AssemblyType.AssemblyCSharpFirstPass, out var assemblyCSharpFirstPassTypes);
            AddTypesFromAssembly(assemblyCSharpFirstPassTypes, interfaceType, types);
            
            return types;
        }

        private static void AddTypesFromAssembly(Type[] assemblyType, Type interfaceType, List<Type> types)
        {
            if(assemblyType == null) return;

            types.AddRange(assemblyType.Where(type => type != interfaceType && interfaceType.IsAssignableFrom(type)));
        }
    }
}