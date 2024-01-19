using Dalamud.Interface.Components;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods.TerritorySelection;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using Hyperborea.ECommons_CNExtra;
using Lumina.Excel.GeneratedSheets;

namespace Hyperborea.Gui;

public unsafe static class UI
{
    public static SavedZoneState SavedZoneState = null;
    public static Vector3? SavedPos = null;
    static int a2 = 0;
    static int a3 = 0;
    static int a4 = 0;
    static int a5 = 1;
    internal static int a6 = 1;
    static Point3 Position = new(0,0,0);
    static bool SpawnOverride;

    public static void DrawNeo()
    {
        var l = LayoutWorld.Instance()->ActiveLayout;
        var disableCheckbox = !Utils.CanEnablePlugin(out var DisableReasons);
        if (disableCheckbox) ImGui.BeginDisabled();
        if (ImGui.Checkbox("Enable Hyperborea", ref P.Enabled))
        {
            if (P.Enabled)
            {
                SavedPos = Player.Object.Position;
                P.Memory.EnableFirewall();
                P.Memory.TargetSystem_InteractWithObjectHook.Enable();
            }
            else
            {
                Utils.Revert();
                SavedPos = null;
                SavedZoneState = null;
                P.Memory.DisableFirewall();
                P.Memory.TargetSystem_InteractWithObjectHook.Pause();
            }
        }

        if (disableCheckbox)
        {
            ImGui.EndDisabled();
            ImGuiEx_Extend.HelpMarker($"Hyperborea cannot be enabled as you are under the following restricted condition(s):\n{DisableReasons.Print("\n")}", ImGuiColors.DalamudOrange);
        }

        ImGuiEx.Text("Packet Filter:");
        ImGui.SameLine();
        if (P.Memory.PacketDispatcher_OnSendPacketHook.IsEnabled && P.Memory.PacketDispatcher_OnReceivePacketHook.IsEnabled)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text(EColor.GreenBright, FontAwesomeIcon.Check.ToIconString());
            ImGui.PopFont();
        }
        else
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text(EColor.RedBright, "\uf00d");
            ImGui.PopFont();
        }
        ImGuiEx.Tooltip("When Hyperborea's packet filter is enabled, your packets to and from the game server are filtered to only prevent the client kicking you out to the lobby.");
        ImGui.SameLine();

        ImGuiEx.Text("Interact Hook:");
        ImGui.SameLine();
        if (P.Memory.TargetSystem_InteractWithObjectHook.IsEnabled)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text(EColor.GreenBright, FontAwesomeIcon.Check.ToIconString());
            ImGui.PopFont();
        }
        else
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text(EColor.RedBright, "\uf00d");
            ImGui.PopFont();
        }
        ImGuiEx.Tooltip("When Hyperborea's interact hook is enabled, you will be unable to interact with EventNpcs/EventObjs.");

        ImGuiEx.Text("Free Trial:");
        ImGui.SameLine();
        if (Svc.Condition[ConditionFlag.OnFreeTrial])
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text(EColor.GreenBright, FontAwesomeIcon.Check.ToIconString());
            ImGui.PopFont();
        }
        else
        {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGuiEx.Text(EColor.RedBright, "\uf00d");
            ImGui.PopFont();
        }
        ImGuiEx.Tooltip("While Hyperborea attempts to implement safety as much as possible by preventing sending data to server while using it, no guarantees is given and it's always recommended to use it with free trial account.");

        if (ImGuiGroup.BeginGroupBox())
        {
            try
            {
                ZoneInfo info = null;
                var layout = Utils.GetLayout();
                Utils.TryGetZoneData(layout, out info);

                var cur = ImGui.GetCursorPos();
                ImGui.SetCursorPosX(ImGuiEx.GetWindowContentRegionWidth() - ImGuiHelpers.GetButtonSize("Browse").X - ImGuiHelpers.GetButtonSize("Zone Editor").X - 50f.Scale());
                if (ImGuiComponents.IconButtonWithText((FontAwesomeIcon)0xf002, "Browse"))
                {
                    new TerritorySelector((uint)a2, (sel, x) =>
                    {
                        a2 = (int)x;
                    });
                }
                ImGui.SameLine();
                if (ImGuiComponents.IconButtonWithText((FontAwesomeIcon)0xf303, "Zone Editor"))
                {
                    P.EditorWindow.IsOpen = true;
                    P.EditorWindow.SelectedTerritory = (uint)a2;
                }

                ImGui.SetCursorPos(cur);
                ImGuiEx.TextV("Zone Data:");
                ImGuiEx_Extend.SetNextItemWidthScaled(150);
                var dis = TerritorySelector.Selectors.Any(x => x.IsOpen);
                if (dis) ImGui.BeginDisabled();
                ImGui.InputInt("Territory Type ID", ref a2);
                if (dis) ImGui.EndDisabled();
                if (ExcelTerritoryHelper_Extend.NameExists((uint)a2))
                {
                    ImGuiEx.Text(ExcelTerritoryHelper.GetName((uint)a2));
                }
                ImGuiEx.Text($"Additional Data:");
                ImGuiEx_Extend.SetNextItemWidthScaled(150);
                var StoryValues = Utils.GetStoryValues((uint)a2);
                var disableda3 = !StoryValues.Any(x => x != 0);
                if (disableda3) ImGui.BeginDisabled();
                if (ImGui.BeginCombo("Story Progress", $"{a3}"))
                {
                    foreach (var x in StoryValues.Order())
                    {
                        if (ImGui.Selectable($"{x}", a3 == x)) a3 = (int)x;
                        if (a3 == x && ImGui.IsWindowAppearing()) ImGui.SetScrollHereY();
                    }
                    ImGui.EndCombo();
                }
                if (disableda3) ImGui.EndDisabled();
                if (!StoryValues.Contains((uint)a3)) a3 = (int)StoryValues.FirstOrDefault();
                ImGuiEx_Extend.SetNextItemWidthScaled(150);
                ImGui.InputInt("Argument 4", ref a4);
                ImGuiEx_Extend.SetNextItemWidthScaled(150);
                ImGui.InputInt("Argument 5", ref a5);

                ImGui.Checkbox($"Spawn Override:", ref SpawnOverride);
                if (!SpawnOverride) ImGui.BeginDisabled();
                CoordBlock("X:", ref Position.X);
                ImGui.SameLine();
                CoordBlock("Y:", ref Position.Y);
                ImGui.SameLine();
                CoordBlock("Z:", ref Position.Z);
                if (!SpawnOverride) ImGui.EndDisabled();

                ImGuiHelpers.ScaledDummy(3f);
                ImGui.Separator();
                ImGuiHelpers.ScaledDummy(3f);

                {
                    var size = ImGuiEx_Extend.CalcIconSize("\uf3c5", true);
                    size += ImGuiEx_Extend.CalcIconSize("\uf15c", true);
                    size += ImGuiEx_Extend.CalcIconSize(FontAwesomeIcon.Cog, true);
                    size.X += ImGui.GetStyle().ItemSpacing.X * 3;

                    var cur2 = ImGui.GetCursorPos();
                    ImGui.SetCursorPosX(ImGuiEx.GetWindowContentRegionWidth() - size.X);
                    var disabled = !Utils.CanUse();
                    if (disabled) ImGui.BeginDisabled();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Compass))
                    {
                        P.CompassWindow.IsOpen = !P.CompassWindow.IsOpen;
                    }
                    if (disabled) ImGui.EndDisabled();
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton("\uf15c"))
                    {
                        P.LogWindow.IsOpen = true;
                    }
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Cog))
                    {
                        P.SettingsWindow.IsOpen = true;
                    }
                    ImGui.SetCursorPos(cur2);
                }

                {
                    var disabled = !Utils.CanUse();
                    if (disabled) ImGui.BeginDisabled();
                    if (ImGui.Button("Load Zone"))
                    {
                        Utils.TryGetZoneData(Utils.GetLayout((uint)a2), out var info2);
                        SavedZoneState ??= new SavedZoneState(l->TerritoryTypeId, Player.Object.Position);
                        Utils.LoadZone((uint)a2, !SpawnOverride, true, a3, a4, a5, a6);
                        if (SpawnOverride)
                        {
                            // Player.GameObject->SetPosition(Position.X, Position.Y, Position.Z);

                            Player.GameObject->Position.X = Position.X;
                            Player.GameObject->Position.Y = Position.Y;
                            Player.GameObject->Position.Z = Position.Z;
                        }
                        else if (info2 != null && info2.Spawn != null)
                        {
                            // Player.GameObject->SetPosition(info2.Spawn.X, info2.Spawn.Y, info2.Spawn.Z);

                            Player.GameObject->Position.X = info2.Spawn.X;
                            Player.GameObject->Position.Y = info2.Spawn.Y;
                            Player.GameObject->Position.Z = info2.Spawn.Z;
                        }
                    }
                    if (disabled) ImGui.EndDisabled();
                }
                ImGui.SameLine();
                {
                    var disabled = !P.Enabled;
                    if (disabled) ImGui.BeginDisabled();
                    if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Undo, "Revert"))
                    {
                        Utils.Revert();
                    }
                    if (disabled) ImGui.EndDisabled();
                }
            }
            catch(Exception e)
            {
                ImGuiEx.Text(e.ToString());
            }
            ImGuiGroup.EndGroupBox();
        }
    }
    internal static void CoordBlock(string t, ref float p)
    {
        ImGuiEx.TextV(t);
        ImGui.SameLine();
        ImGuiEx_Extend.SetNextItemWidthScaled(60f);
        ImGui.DragFloat("##" + t, ref p, 0.1f);
    }
}
