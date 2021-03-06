// -----------------------------------------------------------------------
// <copyright file="CreatingPortal.cs" company="Exiled Team">
// Copyright (c) Exiled Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Events.Patches.Events.Scp106
{
#pragma warning disable SA1118
#pragma warning disable SA1313
    using System.Collections.Generic;

    using System.Reflection.Emit;

    using Exiled.Events.EventArgs;
    using Exiled.Events.Handlers;

    using HarmonyLib;

    using UnityEngine;

    using static HarmonyLib.AccessTools;

    /// <summary>
    /// Patches <see cref="Scp106PlayerScript.CallCmdMakePortal"/>.
    /// Adds the <see cref="Scp106.CreatingPortal"/> event.
    /// </summary>
    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMakePortal))]
    internal static class CreatingPortal
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var newInstructions = new List<CodeInstruction>(instructions);

            // Search for the last "ldarg.0".
            var index = newInstructions.FindLastIndex(instruction => instruction.opcode == OpCodes.Ldarg_0);

            // Declare CreatingPortalEventArgs, to be able to store its instance with "stloc.1".
            generator.DeclareLocal(typeof(CreatingPortalEventArgs));

            // var ev = new CreatingPortalEventArgs(API.Features.Player.Get(this.gameObject), raycastHit.point - Vector3.up, true);
            //
            // Scp106.OnCreatingPortal(ev);
            //
            // if (!ev.IsAllowed)
            //   return;
            //
            // this.SetPortalPosition(ev.Position);
            //
            // return;
            newInstructions.InsertRange(index, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Component), nameof(Component.gameObject))),
                new CodeInstruction(OpCodes.Call, Method(typeof(API.Features.Player), nameof(API.Features.Player.Get), new[] { typeof(GameObject) })),
                new CodeInstruction(OpCodes.Ldloca_S, 0),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(RaycastHit), nameof(RaycastHit.point))),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(Vector3), nameof(Vector3.up))),
                new CodeInstruction(OpCodes.Call, Method(typeof(Vector3), "op_Subtraction")),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Newobj, GetDeclaredConstructors(typeof(CreatingPortalEventArgs))[0]),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Stloc_1),
                new CodeInstruction(OpCodes.Call, Method(typeof(Scp106), nameof(Scp106.OnCreatingPortal))),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(CreatingPortalEventArgs), nameof(CreatingPortalEventArgs.IsAllowed))),
                new CodeInstruction(OpCodes.Brfalse_S, newInstructions[index - 1].operand),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(CreatingPortalEventArgs), nameof(CreatingPortalEventArgs.Position))),
                new CodeInstruction(OpCodes.Call, Method(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.SetPortalPosition))),
                new CodeInstruction(OpCodes.Ret),
            });

            return newInstructions;
        }
    }
}
