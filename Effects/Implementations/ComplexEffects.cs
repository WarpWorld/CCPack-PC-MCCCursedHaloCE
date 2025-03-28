﻿using System.Diagnostics;
using CrowdControl.Common;
using CrowdControl.Games.Packs.MCCCursedHaloCE.Effects;
using CrowdControl.Games.Packs.MCCCursedHaloCE.Utilities.InputEmulation;

namespace CrowdControl.Games.Packs.MCCCursedHaloCE;

public partial class MCCCursedHaloCE
{
    private void Thunderstorm(int totalDurationInMilliseconds, int fadeInDurationInMs, int fadeOutDurationInMs, int delayAfterFadeIn, int delayAfterFadeOut)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < totalDurationInMilliseconds)
        {
            QueueOneShotEffect((short)OneShotEffect.StormyNight_FadeOut, fadeOutDurationInMs);
            Thread.Sleep(delayAfterFadeOut + fadeOutDurationInMs);
            QueueOneShotEffect((short)OneShotEffect.StormyNight_FadeIn, fadeInDurationInMs);
            Thread.Sleep(delayAfterFadeIn + fadeInDurationInMs);
        }
        QueueOneShotEffect((short)OneShotEffect.StormyNight_FadeIn, 0);
        stopwatch.Stop();
    }

    private void Paranoia(EffectRequest request, int delayBetweenFlashlightToggleInMs)
    {
        int totalDurationInMs = (int)request.Duration.TotalMilliseconds;
        bool flashlightState = false;
        var act = RepeatAction(request,
            () => IsReady(request),
            () =>
            {
                QueueOneShotEffect((short)OneShotEffect.Paranoia_Start, totalDurationInMs);

                return true;
            },
            TimeSpan.FromSeconds(1),
            () => IsReady(request),
            TimeSpan.FromSeconds(1),
            () =>
            {
                if (flashlightState == true)
                {
                    QueueOneShotEffect((short)OneShotEffect.Flashlight_Off, 0);
                }
                else
                {
                    QueueOneShotEffect((short)OneShotEffect.Flashlight_On, 0);
                }

                flashlightState = !flashlightState;

                return true;
            },
            TimeSpan.FromMilliseconds(delayBetweenFlashlightToggleInMs),
            false,
            new string[] { EffectMutex.UI });            
    }

    private void Berserker(EffectRequest request)
    {
        var act = StartTimed(request,
            () => IsReady(request) && keyManager.EnsureKeybindsInitialized(halo1BaseAddress),
            () => IsReady(request),
            TimeSpan.FromMilliseconds(3000),
            () =>
            {
                // Keybinds
                // In Cursed, I no longer disable Fire nor set it to melee since inferno made a fists weapon that usees both.
                // keyManager.SetAlernativeBindingToOTherActions(GameAction.Melee, GameAction.Fire);
                // keyManager.DisableAction(GameAction.Fire);
                keyManager.DisableAction(GameAction.ThrowGrenade);
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();

                Connector.SendMessage($"{request.DisplayViewer} told you to RIP AND TEAR.");
                    
                // Movement speed
                SetPlayerMovementSpeedWithoutEffect(1.5f);

                // Includes one punch one kill and deathless
                QueueOneShotEffect((short)OneShotEffect.Berserk, (int)request.Duration.TotalMilliseconds);

                return true;
            },
            new string[] { EffectMutex.PlayerSpeed, EffectMutex.PlayerReceivedDamage, EffectMutex.Ammo, EffectMutex.KeyDisable, EffectMutex.KeyPress });
        act.WhenCompleted.Then(_ =>
        {
            // Keybinds
            keyManager.RestoreAllKeyBinds();
            keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
            keyManager.ResetAlternativeBindingForAction(GameAction.Melee, halo1BaseAddress);
            BringGameToForeground();
            keyManager.ForceShortPause();

            Connector.SendMessage($"You can calm down now.");
            // Repair health and shields.
            SetHealth(1, true);
            SetShield(1);

            // Reset speed.
            SetPlayerMovementSpeedWithoutEffect(1);
        });
    }
}