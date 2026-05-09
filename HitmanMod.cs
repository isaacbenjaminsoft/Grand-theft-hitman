using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

public class HitContract : Script
{
    private Ped target;
    private Blip targetBlip;
    private readonly List<Ped> bodyguards = new List<Ped>();
    private readonly Random rng = new Random();
    private bool active;

    private const float radius = 120f;

    public HitContract()
    {
        Tick += OnTick;
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F10 && !active)
        {
            StartContract();
        }
    }

    private void StartContract()
    {
        Ped player = Game.Player.Character;
        Ped[] peds = World.GetNearbyPeds(player.Position, radius);

        List<Ped> valid = new List<Ped>();

        foreach (Ped p in peds)
        {
            if (p != null && p.Exists() && p.IsAlive && !p.IsPlayer)
            {
                valid.Add(p);
            }
        }

        if (valid.Count == 0)
        {
            UI.Notify("No valid targets nearby.");
            return;
        }

        target = valid[rng.Next(valid.Count)];

        targetBlip = target.AddBlip();
        targetBlip.Color = BlipColor.Red;
        targetBlip.Name = "Target";

        SpawnBodyguards();

        active = true;
        UI.Notify("Contract Started: Eliminate the target.");
    }

    private void SpawnBodyguards()
    {
        bodyguards.Clear();

        Model copModel = new Model("s_m_y_cop_01");
        

        copModel.Request(1000);
        if (!copModel.IsInCdImage || !copModel.IsValid) return;

        while (!copModel.IsLoaded) Script.Wait(100);

        Vector3 offset1 = new Vector3(1.5f, 0f, 0f);
        Vector3 offset2 = new Vector3(-1.5f, 0f, 0f);

        Ped guard1 = World.CreatePed(copModel, target.Position + offset1, target.Heading);
        Ped guard2 = World.CreatePed(copModel, target.Position + offset2, target.Heading);


        WeaponHash pistolHash = (WeaponHash)Game.GenerateHash("WEAPON_PISTOL");

        if (guard1 != null && guard1.Exists())
        {
            guard1.Weapons.Give(pistolHash, 500, true, true);
            guard1.Task.GuardCurrentPosition(); 
            bodyguards.Add(guard1);
        }

        if (guard2 != null && guard2.Exists())
        {
            guard2.Weapons.Give(pistolHash, 500, true, true);
            guard2.Task.GuardCurrentPosition();
            bodyguards.Add(guard2);
        }


        copModel.MarkAsNoLongerNeeded();
    }

    private void OnTick(object sender, EventArgs e)
    {
        if (!active || target == null)
        {
            return;
        }


        if (!target.Exists() || target.IsDead)
        {
            UI.Notify("Contract Complete.");
            Script.Wait(2000); // Brief pause before cleanup
            End();
        }
    }

    private void End()
    {
        if (targetBlip != null && targetBlip.Exists())
        {
            targetBlip.Remove();
        }


        foreach (Ped guard in bodyguards)
        {
            if (guard != null && guard.Exists())
            {
                guard.MarkAsNoLongerNeeded(); // Let the game clean them up naturally
            }
        }

        bodyguards.Clear();
        target = null;
        targetBlip = null;
        active = false;
    }
}