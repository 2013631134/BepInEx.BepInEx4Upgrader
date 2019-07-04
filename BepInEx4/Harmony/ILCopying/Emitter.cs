﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Harmony.ILCopying
{
    public static class Emitter
    {
        private static readonly GetterHandler codeLenGetter =
            FastAccess.CreateFieldGetter(typeof(ILGenerator), "code_len", "m_length");

        private static readonly GetterHandler
            localsGetter = FastAccess.CreateFieldGetter(typeof(ILGenerator), "locals");

        private static readonly GetterHandler localCountGetter =
            FastAccess.CreateFieldGetter(typeof(ILGenerator), "m_localCount");

        public static string CodePos(ILGenerator il)
        {
            var num = (int) codeLenGetter(il);
            return $"L_{num:x4}: ";
        }

        public static void LogIL(ILGenerator il, OpCode opCode, object argument)
        {
            if (HarmonyInstance.DEBUG)
            {
                var text = FormatArgument(argument);
                var text2 = text.Length > 0 ? " " : "";
                FileLog.LogBuffered($"{CodePos(il)}{opCode}{text2}{text}");
            }
        }

        public static void LogLocalVariable(ILGenerator il, LocalBuilder variable)
        {
            if (HarmonyInstance.DEBUG)
            {
                var array = localsGetter != null ? (LocalBuilder[]) localsGetter(il) : null;
                int num;
                if (array != null && array.Length != 0)
                    num = array.Length;
                else
                    num = (int) localCountGetter(il);
                FileLog.LogBuffered(
                    $"{CodePos(il)}Local var {num - 1}: {variable.LocalType.FullName}{(variable.IsPinned ? "(pinned)" : "")}");
            }
        }

        public static string FormatArgument(object argument)
        {
            if (argument == null) return "NULL";
            var type = argument.GetType();
            if (type == typeof(string)) return "\"" + argument + "\"";
            if (type == typeof(Label)) return "Label" + ((Label) argument).GetHashCode();
            if (type == typeof(Label[]))
                return "Labels" + string.Join(",", (from l in (Label[]) argument
                           select l.GetHashCode().ToString()).ToArray());
            if (type == typeof(LocalBuilder))
                return string.Concat(((LocalBuilder) argument).LocalIndex, " (", ((LocalBuilder) argument).LocalType,
                    ")");
            return argument.ToString().Trim();
        }

        public static void MarkLabel(ILGenerator il, Label label)
        {
            if (HarmonyInstance.DEBUG) FileLog.LogBuffered(CodePos(il) + FormatArgument(label));
            il.MarkLabel(label);
        }

        public static void MarkBlockBefore(ILGenerator il, ExceptionBlock block, out Label? label)
        {
            label = null;
            switch (block.blockType)
            {
                case ExceptionBlockType.BeginExceptionBlock:
                    if (HarmonyInstance.DEBUG)
                    {
                        FileLog.LogBuffered(".try");
                        FileLog.LogBuffered("{");
                        FileLog.ChangeIndent(1);
                    }

                    label = il.BeginExceptionBlock();
                    return;
                case ExceptionBlockType.BeginCatchBlock:
                    if (HarmonyInstance.DEBUG)
                    {
                        LogIL(il, OpCodes.Leave, new LeaveTry());
                        FileLog.ChangeIndent(-1);
                        FileLog.LogBuffered("} // end try");
                        FileLog.LogBuffered(".catch " + block.catchType);
                        FileLog.LogBuffered("{");
                        FileLog.ChangeIndent(1);
                    }

                    il.BeginCatchBlock(block.catchType);
                    return;
                case ExceptionBlockType.BeginExceptFilterBlock:
                    if (HarmonyInstance.DEBUG)
                    {
                        LogIL(il, OpCodes.Leave, new LeaveTry());
                        FileLog.ChangeIndent(-1);
                        FileLog.LogBuffered("} // end try");
                        FileLog.LogBuffered(".filter");
                        FileLog.LogBuffered("{");
                        FileLog.ChangeIndent(1);
                    }

                    il.BeginExceptFilterBlock();
                    return;
                case ExceptionBlockType.BeginFaultBlock:
                    if (HarmonyInstance.DEBUG)
                    {
                        LogIL(il, OpCodes.Leave, new LeaveTry());
                        FileLog.ChangeIndent(-1);
                        FileLog.LogBuffered("} // end try");
                        FileLog.LogBuffered(".fault");
                        FileLog.LogBuffered("{");
                        FileLog.ChangeIndent(1);
                    }

                    il.BeginFaultBlock();
                    return;
                case ExceptionBlockType.BeginFinallyBlock:
                    if (HarmonyInstance.DEBUG)
                    {
                        LogIL(il, OpCodes.Leave, new LeaveTry());
                        FileLog.ChangeIndent(-1);
                        FileLog.LogBuffered("} // end try");
                        FileLog.LogBuffered(".finally");
                        FileLog.LogBuffered("{");
                        FileLog.ChangeIndent(1);
                    }

                    il.BeginFinallyBlock();
                    return;
                default:
                    return;
            }
        }

        public static void MarkBlockAfter(ILGenerator il, ExceptionBlock block)
        {
            if (block.blockType == ExceptionBlockType.EndExceptionBlock)
            {
                if (HarmonyInstance.DEBUG)
                {
                    LogIL(il, OpCodes.Leave, new LeaveTry());
                    FileLog.ChangeIndent(-1);
                    FileLog.LogBuffered("} // end handler");
                }

                il.EndExceptionBlock();
            }
        }

        public static void Emit(ILGenerator il, OpCode opcode)
        {
            if (HarmonyInstance.DEBUG) FileLog.LogBuffered(CodePos(il) + opcode);
            il.Emit(opcode);
        }

        public static void Emit(ILGenerator il, OpCode opcode, LocalBuilder local)
        {
            LogIL(il, opcode, local);
            il.Emit(opcode, local);
        }

        public static void Emit(ILGenerator il, OpCode opcode, FieldInfo field)
        {
            LogIL(il, opcode, field);
            il.Emit(opcode, field);
        }

        public static void Emit(ILGenerator il, OpCode opcode, Label[] labels)
        {
            LogIL(il, opcode, labels);
            il.Emit(opcode, labels);
        }

        public static void Emit(ILGenerator il, OpCode opcode, Label label)
        {
            LogIL(il, opcode, label);
            il.Emit(opcode, label);
        }

        public static void Emit(ILGenerator il, OpCode opcode, string str)
        {
            LogIL(il, opcode, str);
            il.Emit(opcode, str);
        }

        public static void Emit(ILGenerator il, OpCode opcode, float arg)
        {
            LogIL(il, opcode, arg);
            il.Emit(opcode, arg);
        }

        public static void Emit(ILGenerator il, OpCode opcode, byte arg)
        {
            LogIL(il, opcode, arg);
            il.Emit(opcode, arg);
        }

        public static void Emit(ILGenerator il, OpCode opcode, sbyte arg)
        {
            LogIL(il, opcode, arg);
            il.Emit(opcode, arg);
        }

        public static void Emit(ILGenerator il, OpCode opcode, double arg)
        {
            LogIL(il, opcode, arg);
            il.Emit(opcode, arg);
        }

        public static void Emit(ILGenerator il, OpCode opcode, int arg)
        {
            LogIL(il, opcode, arg);
            il.Emit(opcode, arg);
        }

        public static void Emit(ILGenerator il, OpCode opcode, MethodInfo meth)
        {
            LogIL(il, opcode, meth);
            il.Emit(opcode, meth);
        }

        public static void Emit(ILGenerator il, OpCode opcode, short arg)
        {
            LogIL(il, opcode, arg);
            il.Emit(opcode, arg);
        }

        public static void Emit(ILGenerator il, OpCode opcode, SignatureHelper signature)
        {
            LogIL(il, opcode, signature);
            il.Emit(opcode, signature);
        }

        public static void Emit(ILGenerator il, OpCode opcode, ConstructorInfo con)
        {
            LogIL(il, opcode, con);
            il.Emit(opcode, con);
        }

        public static void Emit(ILGenerator il, OpCode opcode, Type cls)
        {
            LogIL(il, opcode, cls);
            il.Emit(opcode, cls);
        }

        public static void Emit(ILGenerator il, OpCode opcode, long arg)
        {
            LogIL(il, opcode, arg);
            il.Emit(opcode, arg);
        }

        public static void EmitCall(ILGenerator il, OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
        {
            if (HarmonyInstance.DEBUG)
                FileLog.LogBuffered(string.Format("{0}Call {1} {2} {3}", CodePos(il), opcode, methodInfo,
                    optionalParameterTypes));
            il.EmitCall(opcode, methodInfo, optionalParameterTypes);
        }

        public static void EmitCalli(ILGenerator il, OpCode opcode, CallingConvention unmanagedCallConv,
            Type returnType, Type[] parameterTypes)
        {
            if (HarmonyInstance.DEBUG)
                FileLog.LogBuffered(string.Format("{0}Calli {1} {2} {3} {4}", CodePos(il), opcode, unmanagedCallConv,
                    returnType, parameterTypes));
            il.EmitCalli(opcode, unmanagedCallConv, returnType, parameterTypes);
        }

        public static void EmitCalli(ILGenerator il, OpCode opcode, CallingConventions callingConvention,
            Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
        {
            if (HarmonyInstance.DEBUG)
                FileLog.LogBuffered(string.Format("{0}Calli {1} {2} {3} {4} {5}", CodePos(il), opcode,
                    callingConvention, returnType, parameterTypes, optionalParameterTypes));
            il.EmitCalli(opcode, callingConvention, returnType, parameterTypes, optionalParameterTypes);
        }

        public static void EmitWriteLine(ILGenerator il, string value)
        {
            if (HarmonyInstance.DEBUG)
                FileLog.LogBuffered($"{CodePos(il)}WriteLine {FormatArgument(value)}");
            il.EmitWriteLine(value);
        }

        public static void EmitWriteLine(ILGenerator il, LocalBuilder localBuilder)
        {
            if (HarmonyInstance.DEBUG)
                FileLog.LogBuffered($"{CodePos(il)}WriteLine {FormatArgument(localBuilder)}");
            il.EmitWriteLine(localBuilder);
        }

        public static void EmitWriteLine(ILGenerator il, FieldInfo fld)
        {
            if (HarmonyInstance.DEBUG)
                FileLog.LogBuffered($"{CodePos(il)}WriteLine {FormatArgument(fld)}");
            il.EmitWriteLine(fld);
        }
    }
}