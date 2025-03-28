﻿using ConnectorLib;
using CrowdControl.Common;
using CrowdControl.Games.Packs.MCCCursedHaloCE.Effects;
using CrowdControl.Games.Packs.MCCCursedHaloCE.Utilities.InputEmulation;

namespace CrowdControl.Games.Packs.MCCCursedHaloCE;

public partial class MCCCursedHaloCE
{
    // Forces sideways movement.
    public void CrabRave(EffectRequest request)
    {
        KeyManipulationPerFrameEffect(request,
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} made you walk like a crab.");
                keyManager.DisableAction(GameAction.RunForward);
                keyManager.DisableAction(GameAction.RunBackwards);
                keyManager.DisableAction(GameAction.StrafeRight);
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);

                BringGameToForeground();
                keyManager.ForceShortPause();
                return true;
            },
            (task) =>
            {
                keyManager.RestoreAllKeyBinds();
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.SendAction(GameAction.StrafeLeft, true);
                keyManager.ForceShortPause();
                Connector.SendMessage($"Crabness expunged.");
            },
            () =>
            {
                BringGameToForeground();
                return keyManager.SendAction(GameAction.StrafeLeft, false);
            });
    }

    // Forces backwards movement.
    public void Moonwalk(EffectRequest request)
    {
        KeyManipulationPerFrameEffect(request,
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} made your pronouns \"he/hee\".");
                keyManager.DisableAction(GameAction.RunForward);
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);

                BringGameToForeground();
                keyManager.ForceShortPause();
                return true;
            },
            (task) =>
            {
                keyManager.RestoreAllKeyBinds();
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.SendAction(GameAction.RunBackwards, true);
                keyManager.ForceShortPause();
                Connector.SendMessage($"Forward movement is now allowed.");
            },
            () =>
            {
                BringGameToForeground();
                return keyManager.SendAction(GameAction.RunBackwards, false);
            });
    }

    // Forces jumping.
    public void BunnyHop(EffectRequest request)
    {
        KeyManipulationPerFrameEffect(request,
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} put some literal spring on your step.");
                return true;
            },
            (task) =>
            {
                keyManager.SendAction(GameAction.Jump, true);
                Connector.SendMessage($"Floor is no longer lava.");
            },
            () =>
            {
                BringGameToForeground();
                return keyManager.SendAction(GameAction.Jump, false);
            });
    }

    // Enables double jump and makes you flap.
    public void FlappySpartan(EffectRequest request)
    {
        bool keyUp = true; // Used to alternate events on each frame.
        RepeatAction(request,
            () => IsReady(request) && keyManager.EnsureKeybindsInitialized(halo1BaseAddress),
            () =>
            {
                QueueOneShotEffect((short)OneShotEffect.FlappySpartan_Start, (int)request.Duration.TotalMilliseconds);

                Connector.SendMessage($"{request.DisplayViewer} is playing Flappy Spartan.");
                return true;
            },
            TimeSpan.FromSeconds(1),
            () => IsInGameplay(),
            TimeSpan.FromMilliseconds(500),
            () =>
            {
                BringGameToForeground();
                keyUp = !keyUp;
                QueueOneShotEffect((short)OneShotEffect.FlappySpartan_Update, 0);

                return keyManager.SendAction(GameAction.Jump, keyUp);
            },
            TimeSpan.FromMilliseconds(150),
            false,
            new string[] { EffectMutex.KeyPress, EffectMutex.KeyDisable, EffectMutex.Ammo, EffectMutex.Gravity }).WhenCompleted.Then((task) =>
        {
            Connector.SendMessage($"You hit a pipe.");
            keyManager.SendAction(GameAction.Jump, true);
        });
    }

    // Forces constant crouching.
    public void ForceCrouch(EffectRequest request)
    {
        KeyManipulationPerFrameEffect(request,
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} broke your ankles.");
                if (!keyManager.SwapActionWithArbitraryKeyCode(GameAction.Crouch, HIDConnector.VirtualKeyCode.NUMPAD1))
                {
                    Connector.SendMessage("Could not swap crouch to an unused key.");
                }

                keyManager.DisableAction(GameAction.Jump);
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                return true;
            },
            (task) =>
            {
                keyManager.RestoreAllKeyBinds();
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.SendAction(GameAction.Crouch, true);
                keyManager.ForceShortPause();
                Connector.SendMessage($"You can move normally again.");
            },
            () =>
            {
                BringGameToForeground();
                return keyManager.SendAction(GameAction.Crouch, false);
            });
    }

    // Forces constant grenadet throws.
    public void ForceGrenades(EffectRequest request)
    {
        bool keyUp = true;
        RepeatAction(request,
            () => IsReady(request) && keyManager.EnsureKeybindsInitialized(halo1BaseAddress),
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} told you to get rid of your grenades.");
                return true;
            },
            TimeSpan.FromSeconds(1),
            () => IsInGameplay(),
            TimeSpan.FromMilliseconds(500),
            () =>
            {
                BringGameToForeground();
                keyUp = !keyUp;
                return keyManager.SendAction(GameAction.ThrowGrenade, keyUp);
            },
            TimeSpan.FromMilliseconds(33),
            false,
            new string[] { EffectMutex.KeyPress, EffectMutex.KeyDisable }).WhenCompleted.Then((task) =>
        {
            keyManager.SendAction(GameAction.ThrowGrenade, true);
            Connector.SendMessage($"Enough grenading, soldier!.");
        });
    }        

    // Prevents firing, melee and grenades.
    public void Pacifist(EffectRequest request)
    {
        KeyManipulationEffect(request,
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} tells you to take it easy, man.");
                keyManager.DisableAction(GameAction.Melee);
                keyManager.DisableAction(GameAction.Fire);
                keyManager.DisableAction(GameAction.ThrowGrenade);
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                return true;
            },
            (task) =>
            {
                keyManager.RestoreAllKeyBinds();
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                Connector.SendMessage($"Violence is allowed again.");
            });
    }

    // Prevents usage of movement keys.
    public void TurretMode(EffectRequest request)
    {
        KeyManipulationEffect(request,
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} ordered you to stay put.");
                QueueOneShotEffect((short)OneShotEffect.TrulyInfiniteAmmoNoSound, (int)request.Duration.TotalMilliseconds);
                keyManager.DisableAction(GameAction.RunBackwards);
                keyManager.DisableAction(GameAction.RunForward);
                keyManager.DisableAction(GameAction.StrafeLeft);
                keyManager.DisableAction(GameAction.StrafeRight);
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                return true;
            },
            (task) =>
            {
                keyManager.RestoreAllKeyBinds();
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                Connector.SendMessage($"You can move again.");
            });
    }

    // Swaps A with D, and W with S
    public void ReverseMovementKeys(EffectRequest request)
    {
        KeyManipulationEffect(request,
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} confused your legs.");
                keyManager.ReverseMovementKeys(halo1BaseAddress);
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                return true;
            },
            (task) =>
            {
                keyManager.RestoreAllKeyBinds();
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                Connector.SendMessage($"Legs are fine again.");
            },
            new string[] { EffectMutex.KeyChange });
    }

    // Randomize the current key bindings among each other, excluding WASD.
    public void RandomizeControls(EffectRequest request)
    {
        KeyManipulationEffect(request,
            () =>
            {
                Connector.SendMessage($"{request.DisplayViewer} randomized your controls.");
                keyManager.RandomizeNonRunningKeys(halo1BaseAddress);
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                return true;
            },
            (task) =>
            {
                keyManager.RestoreAllKeyBinds();
                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                Connector.SendMessage($"Controls are sane again.");
            },
            new string[] { EffectMutex.KeyChange });
    }

    // Template for effects that press, change or disable keys that need to do something every frame.
    private void KeyManipulationPerFrameEffect(EffectRequest request, Func<bool> startAction, Action<Task> endAction, Func<bool> perFrameAction)
    {
        RepeatAction(request,
            () => IsReady(request) && keyManager.EnsureKeybindsInitialized(halo1BaseAddress),
            startAction,
            TimeSpan.FromSeconds(1),
            () => IsInGameplay(),
            TimeSpan.FromMilliseconds(500),
            perFrameAction,
            TimeSpan.FromMilliseconds(33),
            false,
            new string[] { EffectMutex.KeyDisable, EffectMutex.KeyPress }).WhenCompleted.Then(endAction);
    }

    // Template for effects that press, change or disable keys that don't do anything else in between start and end.
    private void KeyManipulationEffect(EffectRequest request, Func<bool> startAction, Action<Task> endAction, string[] specialCaseMutex = null)
    {
        string[] mutex = specialCaseMutex != null ? specialCaseMutex : new string[] { EffectMutex.KeyPress, EffectMutex.KeyDisable };
        RepeatAction(request,
            () => IsReady(request) && keyManager.EnsureKeybindsInitialized(halo1BaseAddress),
            startAction,
            TimeSpan.FromSeconds(1),
            () => true,
            TimeSpan.FromMilliseconds(500),
            () => true,
            TimeSpan.FromMilliseconds(1000),
            false,
            mutex).WhenCompleted.Then(endAction);
    }

    // Forces fire and sets time between shots to 0.
    // If unlimitedAmmo is true, it give infinite ammo, clip, battery and heat.
    // Else, it prevents shotgun pumping and sets charge to max so the battery rifle heats up as usual.
    public void FullAuto(EffectRequest request, bool unlimitedAmmo)
    {
        bool keyUp = true; // Used to alternate events on each frame.
        RepeatAction(request,
            () => IsReady(request) && keyManager.EnsureKeybindsInitialized(halo1BaseAddress),
            () =>
            {
                InjectFullerAuto();
                if (unlimitedAmmo)
                {
                    QueueOneShotEffect((short)OneShotEffect.FullestAuto, (int)request.Duration.TotalMilliseconds);
                }

                if (!keyManager.SwapActionWithArbitraryKeyCode(GameAction.Fire, HIDConnector.VirtualKeyCode.NUMPAD0))
                {
                    Connector.SendMessage("Could not swap fire to an unused key.");
                }

                if (!keyManager.SwapActionWithArbitraryKeyCode(GameAction.SwapWeapons, HIDConnector.VirtualKeyCode.NUMPAD1))
                {
                    Connector.SendMessage("Could not swap 'swap weapons' to an unused key.");
                }

                if (!keyManager.SwapActionWithArbitraryKeyCode(GameAction.Reload, HIDConnector.VirtualKeyCode.NUMPAD2))
                {
                    Connector.SendMessage("Could not swap 'reload' to an unused key.");
                }

                keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
                BringGameToForeground();
                keyManager.ForceShortPause();
                Connector.SendMessage($"{request.DisplayViewer} ordered to fire at will. THEIR will.");
                return true;
            },
            TimeSpan.FromSeconds(1),
            () => IsInGameplay(),
            TimeSpan.FromMilliseconds(500),
            () =>
            {
                BringGameToForeground();
                keyUp = !keyUp;

                return keyManager.SendAction(GameAction.Fire, keyUp);
            },
            TimeSpan.FromMilliseconds(33),
            false,
            new string[] { EffectMutex.KeyPress, EffectMutex.KeyDisable, EffectMutex.Ammo }).WhenCompleted.Then((task) =>
        {
            if (unlimitedAmmo)
            {
                QueueOneShotEffect((short)OneShotEffect.FullestAuto_Stop, 0);
            }
            BringGameToForeground();
            keyManager.SendAction(GameAction.Fire, true);
            keyManager.RestoreAllKeyBinds();
            keyManager.UpdateGameMemoryKeyState(halo1BaseAddress);
            keyManager.ForceShortPause();
            UndoInjection(FullerAutoId);
            Connector.SendMessage($"Trigger discipline is now available once more.");
        });
    }
}