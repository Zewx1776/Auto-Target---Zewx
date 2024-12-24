using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Cache;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Helpers;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using static WhereAreYouGoing.WhereAreYouGoingSettings;
using Map = ExileCore2.PoEMemory.Elements.Map;
using RectangleF = ExileCore2.Shared.RectangleF;

namespace WhereAreYouGoing;

public class WhereAreYouGoing : BaseSettingsPlugin<WhereAreYouGoingSettings>
{
    private CachedValue<float> _diag;
    private CachedValue<RectangleF> _mapRect;
    private bool _autoTargetEnabled;
    private Entity _currentTarget;
    private Vector2? _currentTargetPosition;

    private IngameUIElements ingameStateIngameUi;
    private float k;
    private bool largeMap;
    private float scale;
    private Vector2 ScreenCenterCache;
    private Camera Camera => GameController.Game.IngameState.Camera;
    private Map MapWindow => GameController.Game.IngameState.IngameUi.Map;
    private RectangleF CurrentMapRect => _mapRect?.Value ?? (_mapRect = new TimeCache<RectangleF>(() => MapWindow.GetClientRect(), 100)).Value;

    private Vector2 ScreenCenter =>
        new Vector2(CurrentMapRect.Width / 2, CurrentMapRect.Height / 2 - 20) + new Vector2(CurrentMapRect.X, CurrentMapRect.Y) + new Vector2(MapWindow.LargeMapShiftX, MapWindow.LargeMapShiftY);

    private float Diagonal =>
        _diag?.Value ??
        (_diag = new TimeCache<float>(() =>
        {
            if (ingameStateIngameUi.Map.SmallMiniMap.IsVisibleLocal)
            {
                var mapRect = ingameStateIngameUi.Map.SmallMiniMap.GetClientRect();
                return (float)(Math.Sqrt(mapRect.Width * mapRect.Width + mapRect.Height * mapRect.Height) / 2f);
            }

            return (float)Math.Sqrt(Camera.Width * Camera.Width + Camera.Height * Camera.Height);
        }, 100)).Value;

    public override bool Initialise() => true;

    public WAYGConfig SettingMenu(WAYGConfig setting, string prefix)
    {
        var settings = setting;
        if (ImGui.CollapsingHeader($@"{prefix} Entities##{prefix}", ImGuiTreeNodeFlags.Framed | ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Start Indent
            ImGui.Indent();

            settings.Enable = ImGuiExtension.Checkbox($@"{prefix}(s) Enabled", settings.Enable);
            if (ImGui.TreeNode($@"Colors##{prefix}"))
            {
                settings.Colors.MapColor = ImGuiExtension.ColorPicker("Map Color", settings.Colors.MapColor);
                settings.Colors.MapAttackColor = ImGuiExtension.ColorPicker("Map Attack Color", settings.Colors.MapAttackColor);
                settings.Colors.WorldColor = ImGuiExtension.ColorPicker("World Color", settings.Colors.WorldColor);
                settings.Colors.WorldAttackColor = ImGuiExtension.ColorPicker("World Attack Color", settings.Colors.WorldAttackColor);
                ImGui.Spacing();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode($@"Map##{prefix}"))
            {
                settings.Map.Enable = ImGuiExtension.Checkbox("Map Drawing Enabled", settings.Map.Enable);
                settings.Map.DrawAttack = ImGuiExtension.Checkbox("Draw Attacks", settings.Map.DrawAttack);
                settings.Map.DrawDestination = ImGuiExtension.Checkbox("Draw Destinations", settings.Map.DrawDestination);
                var lineThickness = settings.Map.LineThickness;
                if (ImGui.SliderInt("Line Thickness", ref lineThickness, 1, 100))
                {
                    settings.Map.LineThickness = lineThickness;
                }
                ImGui.Spacing();
                ImGui.TreePop();
            }

            if (ImGui.TreeNode($@"World##{prefix}"))
            {
                settings.World.Enable = ImGuiExtension.Checkbox("World Drawing Enabled", settings.World.Enable);
                settings.World.DrawAttack = ImGuiExtension.Checkbox("World Attacks", settings.World.DrawAttack);
                settings.World.DrawAttackEndPoint = ImGuiExtension.Checkbox("World Attack Endpoint", settings.World.DrawAttackEndPoint);
                settings.World.DrawDestination = ImGuiExtension.Checkbox("World Destination", settings.World.DrawDestination);
                settings.World.DrawDestinationEndPoint = ImGuiExtension.Checkbox("World Destination Endpoint", settings.World.DrawDestinationEndPoint);
                settings.World.DrawLine = ImGuiExtension.Checkbox("Draw Line", settings.World.DrawLine);
                settings.World.AlwaysRenderWorldUnit = ImGuiExtension.Checkbox("Always Render Entity Circle", settings.World.AlwaysRenderWorldUnit);
                settings.World.DrawFilledCircle = ImGuiExtension.Checkbox("Draw Filled Circle", settings.World.DrawFilledCircle);
                settings.World.DrawBoundingBox = ImGuiExtension.Checkbox("Draw Bounding Box Instead of Circle Around Unit", settings.World.DrawBoundingBox);
                var renderCircleThickness = settings.World.RenderCircleThickness;
                if (ImGui.SliderInt("Entity Circle Thickness", ref renderCircleThickness, 1, 100))
                {
                    settings.World.RenderCircleThickness = renderCircleThickness;
                }
                var lineThickness = settings.World.LineThickness;
                if (ImGui.SliderInt("Line Thickness", ref lineThickness, 1, 100))
                {
                    settings.World.LineThickness = lineThickness;
                }
                ImGui.Spacing();
                ImGui.TreePop();
            }

            // End Indent
            ImGui.Unindent();
        }

        // Reapply new settings
        return settings;
    }

    public override void DrawSettings()
    {
        base.DrawSettings();

        Settings.Players = SettingMenu(Settings.Players, "Players");
        Settings.Self = SettingMenu(Settings.Self, "Self");
        Settings.Minions = SettingMenu(Settings.Minions, "All Friendlys");
        Settings.NormalMonster = SettingMenu(Settings.NormalMonster, "Normal Monster");
        Settings.MagicMonster = SettingMenu(Settings.MagicMonster, "Magic Monster");
        Settings.RareMonster = SettingMenu(Settings.RareMonster, "Rare Monster");
        Settings.UniqueMonster = SettingMenu(Settings.UniqueMonster, "Unique Monster");
        Settings.TestingUnits = SettingMenu(Settings.TestingUnits, "Testing Units");

        if (ImGui.CollapsingHeader("Pathfinding Settings", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent();
            
            Settings.Pathfinding.EnablePathfinding.Value = ImGuiExtension.Checkbox(
                "Enable Pathfinding", Settings.Pathfinding.EnablePathfinding.Value);
            
            if (Settings.Pathfinding.EnablePathfinding.Value)
            {
                Settings.Pathfinding.PathColor.Value = ImGuiExtension.ColorPicker(
                    "Path Color", Settings.Pathfinding.PathColor.Value);
                
                var pathThickness = Settings.Pathfinding.PathThickness.Value;
                if (ImGui.SliderFloat("Path Thickness", ref pathThickness,
                    Settings.Pathfinding.PathThickness.Min,
                    Settings.Pathfinding.PathThickness.Max))
                {
                    Settings.Pathfinding.PathThickness.Value = pathThickness;
                }
            }
            
            ImGui.Unindent();
        }
    }

    public override void Tick()
    {
        ingameStateIngameUi = GameController.Game.IngameState.IngameUi;
        k = Camera.Width < 1024f ? 1120f : 1024f;

        if (ingameStateIngameUi.Map.SmallMiniMap.IsVisibleLocal)
        {
            var mapRect = ingameStateIngameUi.Map.SmallMiniMap.GetClientRectCache;
            ScreenCenterCache = new Vector2(mapRect.X + mapRect.Width / 2, mapRect.Y + mapRect.Height / 2);
            largeMap = false;
        }
        else if (ingameStateIngameUi.Map.LargeMap.IsVisibleLocal)
        {
            ScreenCenterCache = ScreenCenter;
            largeMap = true;
        }

        scale = k / Camera.Height * Camera.Width * 3f / 4f / MapWindow.LargeMapZoom;
    }

    public override void Render()
    {
        //Any Imgui or Graphics calls go here. This is called after Tick
        if (!Settings.Enable.Value || !GameController.InGame) return;

        var player = GameController?.Player;

        player.TryGetComponent<Positioned>(out var playerPositioned);
        if (playerPositioned == null) return;
        var playerPos = playerPositioned.GridPos;

        player.TryGetComponent<Render>(out var playerRender);
        if (playerRender == null) return;

        var posZ = GameController.Player.Pos.Z;

        if (MapWindow == null) return;
        var mapWindowLargeMapZoom = MapWindow.LargeMapZoom;

        // Get and draw the path to cursor
        var path = GetPathToCursor();
        if (path != null && path.Count > 0)
        {
            DrawPathLines(path, Settings.Pathfinding.PathColor.Value, Settings.Pathfinding.PathThickness.Value);
        }

        // Toggle auto-targeting when hotkey is pressed
        if (Settings.TargetNearestEnemyKey.PressedOnce())
        {
            _autoTargetEnabled = !_autoTargetEnabled;
            
            // Show status message
            var message = _autoTargetEnabled ? "Auto-targeting Enabled" : "Auto-targeting Disabled";
            var color = _autoTargetEnabled ? Color.Green : Color.Red;
            var textPos = new Vector2(GameController.Window.GetWindowRectangleTimeCache.Width / 2f - 100, 
                                    GameController.Window.GetWindowRectangleTimeCache.Height - 100);
            Graphics.DrawText(message, textPos, color);
        }

        // Find and target nearest enemy if enabled
        if (_autoTargetEnabled)
        {
            MoveCursorToTarget();
        }

        var entityLists = new List<IEnumerable<Entity>>
        {
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Error] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.None] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.ServerObject] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Effect] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Light] ?? Enumerable.Empty<Entity>(),
            GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Monster] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Chest] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.SmallChest] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Npc] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Shrine] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.AreaTransition] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Portal] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.QuestObject] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Stash] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Waypoint] ?? Enumerable.Empty<Entity>(),
            GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Player] ?? Enumerable.Empty<Entity>()
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Pet] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.WorldItem] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Resource] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Breach] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.ControlObjects] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.HideoutDecoration] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.CraftUnlock] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Daemon] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.TownPortal] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Monolith] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.MiniMonolith] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.BetrayalChoice] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.IngameIcon] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.LegionMonolith] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Item] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Terrain] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.DelveCraftingBench] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.GuildStash] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.MiscellaneousObjects] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.Door] ?? Enumerable.Empty<Entity>()
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.DoorSwitch] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.ExpeditionRelic] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.ExpeditionRune] ?? Enumerable.Empty<Entity>(),
            //GameController?.EntityListWrapper?.ValidEntitiesByType[EntityType.TriggerableBlockage] ?? Enumerable.Empty<Entity>()
        };

        var entityList = entityLists.SelectMany(list => list).ToList();

        if (entityList == null) return;

        foreach (var entity in entityList)
        {
            if (entity == null) continue;

            var drawSettings = new WAYGConfig();

            switch (entity.Type)
            {
                case EntityType.Monster:
                    switch (entity.IsHostile)
                    {
                        case true:
                            switch (entity.Rarity)
                            {
                                case MonsterRarity.White:
                                    drawSettings = Settings.NormalMonster;
                                    break;

                                case MonsterRarity.Magic:
                                    drawSettings = Settings.MagicMonster;
                                    break;

                                case MonsterRarity.Rare:
                                    drawSettings = Settings.RareMonster;
                                    break;

                                case MonsterRarity.Unique:
                                    drawSettings = Settings.UniqueMonster;
                                    break;
                            }

                            break;

                        case false:
                            drawSettings = Settings.Minions;
                            break;
                    }

                    break;

                case EntityType.Player:
                    if (entity.Address != player?.Address) drawSettings = Settings.Players;
                    else drawSettings = Settings.Self;

                    break;
                case EntityType.None:
                    if (entity.Metadata == "Metadata/Projectiles/Fireball") drawSettings = Settings.TestingUnits;
                    if (entity.Metadata.Contains("LightningArrow")) drawSettings = Settings.TestingUnits;
                    break;
            }

            #region UnitTesting Entity Add

            if (entity.Type != EntityType.Monster && entity.Type != EntityType.Player) drawSettings = Settings.TestingUnits;

            #endregion

            if (!drawSettings.Enable) continue;

            entity.TryGetComponent<Render>(out var renderComp);
            if (renderComp == null) continue;

            #region UnitTesting Drawing

            if (drawSettings.UnitType == UnitType.UnitTesting)
            {
                if (drawSettings.World.AlwaysRenderWorldUnit)
                    switch (drawSettings.World.DrawBoundingBox)
                    {
                        case true:
                            DrawBoundingBoxInWorld(entity.Pos, drawSettings.Colors.WorldColor, renderComp.Bounds, renderComp.Rotation.X);
                            break;
                        case false:
                            DrawCircleInWorldPos(drawSettings.World.DrawFilledCircle, entity.Pos, renderComp.Bounds.X, drawSettings.World.RenderCircleThickness, drawSettings.Colors.WorldColor);
                            break;
                    }
            }

            #endregion

            entity.TryGetComponent<Pathfinding>(out var pathComp);
            if (pathComp == null) continue;

            entity.TryGetComponent<Actor>(out var actorComp);
            if (actorComp == null) continue;

            var shouldDrawCircle = entity.IsAlive && entity.DistancePlayer < Settings.MaxCircleDrawDistance;

            var actionFlag = actorComp.Action;

            switch (actionFlag)
            {
                case (ActionFlags)512:
                case ActionFlags.None:
                case ActionFlags.None | ActionFlags.HasMines:
                    if (drawSettings.World.AlwaysRenderWorldUnit)
                    {
                        if (shouldDrawCircle)
                            switch (drawSettings.World.DrawBoundingBox)
                            {
                                case true:
                                    DrawBoundingBoxInWorld(entity.Pos, drawSettings.Colors.WorldColor, renderComp.Bounds, renderComp.Rotation.X);
                                    break;
                                case false:
                                    DrawCircleInWorldPos(drawSettings.World.DrawFilledCircle, entity.Pos, renderComp.Bounds.X, drawSettings.World.RenderCircleThickness,
                                        drawSettings.Colors.WorldColor);

                                    break;
                            }
                    }

                    break;

                case (ActionFlags)16386: // Rolling
                case ActionFlags.UsingAbility:
                case ActionFlags.UsingAbilityAbilityCooldown:
                case ActionFlags.UsingAbility | ActionFlags.HasMines:
                    var castGridDestination = actorComp.CurrentAction.Destination;

                    if (drawSettings.Map.Enable && drawSettings.Map.DrawAttack)
                    {
                        var entityTerrainHeight = QueryGridPositionToWorldWithTerrainHeight(entity.GridPos);
                        var entityCastTerrainHeight = QueryGridPositionToWorldWithTerrainHeight(castGridDestination);

                        Vector2 position;
                        var castPosConvert = new Vector2();
                        if (largeMap)
                        {
                            position = ScreenCenterCache +
                                       Helper.DeltaInWorldToMinimapDelta(entity.GridPos - playerPos, Diagonal, scale, (entityTerrainHeight.Z - posZ) / (9f / mapWindowLargeMapZoom));

                            castPosConvert = ScreenCenterCache +
                                             Helper.DeltaInWorldToMinimapDelta(new Vector2(castGridDestination.X, castGridDestination.Y) - playerPos, Diagonal, scale,
                                                 (entityCastTerrainHeight.Z - posZ) / (9f / mapWindowLargeMapZoom));
                        }
                        else
                        {
                            position = ScreenCenterCache + Helper.DeltaInWorldToMinimapDelta(entity.GridPos - playerPos, Diagonal, 240f, (entityTerrainHeight.Z - posZ) / 20);
                            castPosConvert = ScreenCenterCache +
                                             Helper.DeltaInWorldToMinimapDelta(new Vector2(castGridDestination.X, castGridDestination.Y) - playerPos, Diagonal, 240f,
                                                 (entityCastTerrainHeight.Z - posZ) / 20);
                        }

                        Graphics.DrawLine(position, castPosConvert, drawSettings.Map.LineThickness, drawSettings.Colors.MapAttackColor);
                    }

                    if (drawSettings.World.Enable && drawSettings.World.DrawAttack)
                    {
                        if (shouldDrawCircle)
                            switch (drawSettings.World.DrawBoundingBox)
                            {
                                case true:
                                    DrawBoundingBoxInWorld(entity.Pos, drawSettings.Colors.WorldAttackColor, renderComp.Bounds, renderComp.Rotation.X);
                                    break;
                                case false:
                                    DrawCircleInWorldPos(drawSettings.World.DrawFilledCircle, entity.Pos, renderComp.Bounds.X, drawSettings.World.RenderCircleThickness,
                                        drawSettings.Colors.WorldAttackColor);

                                    break;
                            }

                        if (drawSettings.World.DrawLine)
                        {
                            var entityScreenCastPosition = GameController.IngameState.Data.GetGridScreenPosition(castGridDestination);
                            var entityWorldPosition = GameController.IngameState.Data.GetGridScreenPosition(entity.GridPos);
                            Graphics.DrawLine(entityWorldPosition, entityScreenCastPosition, drawSettings.World.LineThickness, drawSettings.Colors.WorldAttackColor);
                        }

                        if (drawSettings.World.DrawAttackEndPoint && shouldDrawCircle)
                        {
                            var worldPosFromGrid = new Vector3(castGridDestination.GridToWorld().X, castGridDestination.GridToWorld().Y, 0);
                            DrawCircleInWorldPos(drawSettings.World.DrawFilledCircle,
                                new Vector3(worldPosFromGrid.Xy(), GameController.IngameState.Data.GetTerrainHeightAt(worldPosFromGrid.WorldToGrid())), renderComp.Bounds.X / 3,
                                drawSettings.World.RenderCircleThickness, drawSettings.Colors.WorldAttackColor);
                        }
                    }
                    else
                    {
                        if (drawSettings.World.AlwaysRenderWorldUnit && shouldDrawCircle)
                            switch (drawSettings.World.DrawBoundingBox)
                            {
                                case true:
                                    DrawBoundingBoxInWorld(entity.Pos, drawSettings.Colors.WorldColor, renderComp.Bounds, renderComp.Rotation.X);
                                    break;
                                case false:
                                    DrawCircleInWorldPos(drawSettings.World.DrawFilledCircle, entity.Pos, renderComp.Bounds.X, drawSettings.World.RenderCircleThickness,
                                        drawSettings.Colors.WorldColor);

                                    break;
                            }
                    }

                    break;

                case ActionFlags.AbilityCooldownActive:
                    break;

                case ActionFlags.Dead:
                    break;

                case (ActionFlags)8320: // Moving
                case (ActionFlags)8322: // Moving and casting
                case (ActionFlags)4224:
                case ActionFlags.Moving | ActionFlags.HasMines | (ActionFlags)4224:
                    if (drawSettings.Map.Enable)
                    {
                        var mapPathNodes = new List<Vector2>();

                        if (drawSettings.Map.DrawDestination && pathComp.PathingNodes.Any())
                        {
                            foreach (var pathNode in pathComp.PathingNodes)
                            {
                                var queriedHeight = QueryGridPositionToWorldWithTerrainHeight(pathNode);
                                mapPathNodes.Add(ScreenCenterCache +
                                                 Helper.DeltaInWorldToMinimapDelta(new Vector2(pathNode.X, pathNode.Y) - playerPos, Diagonal, largeMap ? scale : 240f,
                                                     (queriedHeight.Z - posZ) / (largeMap ? 9f / mapWindowLargeMapZoom : 20)));
                            }
                        }

                        if (mapPathNodes.AddOffset(drawSettings.Map.LineThickness).Count > 0)
                        {
                            for (var i = 0; i < mapPathNodes.Count - 1; i++)
                            {
                                Graphics.DrawLine(mapPathNodes[i], mapPathNodes[i + 1], drawSettings.Map.LineThickness, drawSettings.Colors.MapColor);
                            }
                        }
                    }

                    if (drawSettings.World.Enable)
                    {
                        var pathingNodes = pathComp.PathingNodes.ConvertToVector2List();

                        if (drawSettings.World.DrawLine && drawSettings.World.DrawDestination && pathingNodes.Any())
                        {
                            var pathingNodesToWorld = QueryWorldScreenPositionsWithTerrainHeight(pathingNodes);

                            var previousPoint = pathingNodesToWorld.First();
                            foreach (var currentPoint in pathingNodesToWorld.Skip(1))
                            {
                                Graphics.DrawLine(previousPoint, currentPoint, drawSettings.World.LineThickness, drawSettings.Colors.WorldColor);
                                previousPoint = currentPoint;
                            }
                        }

                        if (drawSettings.World.DrawDestinationEndPoint && pathingNodes.Any() && shouldDrawCircle)
                        {
                            var pathingNodesToWorld = QueryWorldScreenPositionsWithTerrainHeight(pathingNodes);
                            var queriedWorldPos = new Vector3(pathingNodes.Last().GridToWorld().X, pathingNodes.Last().GridToWorld().Y, 0);
                            DrawCircleInWorldPos(drawSettings.World.DrawFilledCircle,
                                new Vector3(queriedWorldPos.Xy(), GameController.IngameState.Data.GetTerrainHeightAt(queriedWorldPos.WorldToGrid())), renderComp.Bounds.X / 3,
                                drawSettings.World.RenderCircleThickness, drawSettings.Colors.WorldColor);
                        }

                        if (drawSettings.World.AlwaysRenderWorldUnit && shouldDrawCircle)
                            switch (drawSettings.World.DrawBoundingBox)
                            {
                                case true:
                                    DrawBoundingBoxInWorld(entity.Pos, drawSettings.Colors.WorldColor, renderComp.Bounds, renderComp.Rotation.X);
                                    break;
                                case false:
                                    DrawCircleInWorldPos(drawSettings.World.DrawFilledCircle, entity.Pos, renderComp.Bounds.X, drawSettings.World.RenderCircleThickness,
                                        drawSettings.Colors.WorldColor);

                                    break;
                            }
                    }

                    break;

                case ActionFlags.WashedUpState:
                    // Handle WashedUpState
                    break;
                default:
                    if (drawSettings.World.AlwaysRenderWorldUnit)
                    {
                        if (shouldDrawCircle)
                            switch (drawSettings.World.DrawBoundingBox)
                            {
                                case true:
                                    DrawBoundingBoxInWorld(entity.Pos, drawSettings.Colors.WorldColor, renderComp.Bounds, renderComp.Rotation.X);
                                    break;
                                case false:
                                    DrawCircleInWorldPos(drawSettings.World.DrawFilledCircle, entity.Pos, renderComp.Bounds.X, drawSettings.World.RenderCircleThickness,
                                        drawSettings.Colors.WorldColor);

                                    break;
                            }
                    }

                    //LogMessage($"New Action State: {actionFlag}");
                    break;
            }
        }

        // Draw path to cursor
        if (Settings.Enable.Value && Settings.NormalMonster.World.Enable && Settings.NormalMonster.World.DrawLine)
        {
            var currentPlayer = GameController?.Player;
            if (currentPlayer != null)
            {
                currentPlayer.TryGetComponent<Positioned>(out var currentPlayerPositioned);
                if (currentPlayerPositioned != null)
                {
                    var pathPoints = GetPathToCursor();
                    DrawPathLines(pathPoints, Settings.NormalMonster.Colors.WorldColor, Settings.NormalMonster.World.LineThickness);
                }
            }
        }
    }

    /// <summary>
    ///     Queries the world screen positions with terrain height for the given grid positions.
    /// </summary>
    /// <param name="gridPositions">The grid positions to query.</param>
    /// <returns>The world screen positions with terrain height.</returns>
    private List<Vector2> QueryWorldScreenPositionsWithTerrainHeight(List<Vector2> gridPositions)
    {
        // Query the world screen positions with terrain height for the given grid positions
        return gridPositions.Select(gridPos => Camera.WorldToScreen(QueryGridPositionToWorldWithTerrainHeight(gridPos))).ToList();
    }

    /// <summary>
    ///     Queries the grid position and extracts the corresponding terrain height.
    /// </summary>
    /// <param name="gridPosition">The grid position to query.</param>
    /// <returns>The world position with the extracted terrain height.</returns>
    private Vector3 QueryGridPositionToWorldWithTerrainHeight(Vector2 gridPosition) =>
        // Query the grid position and extract the corresponding world position with terrain height
        new Vector3(gridPosition.GridToWorld(), GameController.IngameState.Data.GetTerrainHeightAt(gridPosition));

    private void DrawCircleInWorldPos(bool drawFilledCircle, Vector3 position, float radius, int thickness, Color color)
    {
        var screensize = new RectangleF
        {
            X = 0,
            Y = 0,
            Width = GameController.Window.GetWindowRectangleTimeCache.Size.X,
            Height = GameController.Window.GetWindowRectangleTimeCache.Size.Y
        };

        var entityPos = RemoteMemoryObject.TheGame.IngameState.Camera.WorldToScreen(position);
        if (IsEntityWithinScreen(entityPos, screensize, 50))
        {
            if (drawFilledCircle)
            {
                Graphics.DrawFilledCircleInWorld(position, radius, color);
            }
            else
            {
                Graphics.DrawCircleInWorld(position, radius, color, thickness);
            }
        }
    }

    private void DrawBoundingBoxInWorld(Vector3 position, Color color, Vector3 bounds, float rotationRadians)
    {
        var screensize = new RectangleF
        {
            X = 0,
            Y = 0,
            Width = GameController.Window.GetWindowRectangleTimeCache.Size.X,
            Height = GameController.Window.GetWindowRectangleTimeCache.Size.Y
        };

        var entityPos = RemoteMemoryObject.TheGame.IngameState.Camera.WorldToScreen(position);
        if (IsEntityWithinScreen(entityPos, screensize, 50))
        {
            Graphics.DrawBoundingBoxInWorld(position, color, bounds, rotationRadians);
        }
    }

    private bool IsEntityWithinScreen(Vector2 entityPos, RectangleF screensize, float allowancePX)
    {
        // Check if the entity position is within the screen bounds with allowance
        var leftBound = screensize.Left - allowancePX;
        var rightBound = screensize.Right + allowancePX;
        var topBound = screensize.Top - allowancePX;
        var bottomBound = screensize.Bottom + allowancePX;

        return entityPos.X >= leftBound && entityPos.X <= rightBound && entityPos.Y >= topBound && entityPos.Y <= bottomBound;
    }

    private bool HasLineOfSight(int x0, int y0, int x1, int y1, int[][] grid)
    {
        // Bresenham's line algorithm to check all cells between two points
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int x = x0;
        int y = y0;
        int n = 1 + dx + dy;
        int x_inc = (x1 > x0) ? 1 : -1;
        int y_inc = (y1 > y0) ? 1 : -1;
        int error = dx - dy;
        dx *= 2;
        dy *= 2;

        // Check if start and end points are within grid bounds
        if (x0 < 0 || y0 < 0 || x1 < 0 || y1 < 0 ||
            x0 >= grid[0].Length || y0 >= grid.Length ||
            x1 >= grid[0].Length || y1 >= grid.Length)
            return false;

        // Check each point along the line
        for (; n > 0; --n)
        {
            // Check if current point is walkable
            if (grid[y][x] == 0)
                return false;

            if (error > 0)
            {
                x += x_inc;
                error -= dy;
            }
            else
            {
                y += y_inc;
                error += dx;
            }
        }

        return true;
    }

    private Vector2? FindNearestMonsterPosition()
    {
        if (!Settings.Enable || GameController?.Game?.IngameState?.Data?.LocalPlayer == null)
            return null;

        var player = GameController.Game.IngameState.Data.LocalPlayer;
        var playerPos = player.GridPos;
        float nearestDistance = float.MaxValue;
        Vector2? nearestPos = null;
        Entity nearestEntity = null;

        var rawPathfindingData = GameController.IngameState.Data.RawPathfindingData;
        if (rawPathfindingData == null)
            return null;

        foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster])
        {
            if (!entity.IsAlive || !entity.IsHostile || !entity.IsTargetable)
                continue;

            entity.TryGetComponent<Positioned>(out var entityPos);
            if (entityPos == null)
                continue;

            var pos = entityPos.GridPos;
            var distance = Vector2.Distance(playerPos, pos);

            if (distance < nearestDistance)
            {
                // Check if we have line of sight to this monster
                bool hasLineOfSight = HasLineOfSight(
                    (int)playerPos.X, (int)playerPos.Y,
                    (int)pos.X, (int)pos.Y,
                    rawPathfindingData
                );

                if (hasLineOfSight)
                {
                    nearestDistance = distance;
                    nearestPos = pos;
                    nearestEntity = entity;
                }
            }
        }

        // If we found a valid target, save it
        if (nearestEntity != null)
        {
            _currentTarget = nearestEntity;
            _currentTargetPosition = nearestPos.Value;
        }
        else
        {
            _currentTarget = null;
            _currentTargetPosition = null;
        }

        return nearestPos;
    }

    private void MoveCursorToTarget()
    {
        if (!Settings.Enable.Value || GameController?.Game?.IngameState?.Data?.LocalPlayer == null)
            return;

        if (_currentTarget == null || _currentTargetPosition == null)
            return;

        // Only move cursor if target is still valid
        if (!_currentTarget.IsValid || !_currentTarget.IsAlive || !_currentTarget.IsHostile || !_currentTarget.IsTargetable)
        {
            _currentTarget = null;
            _currentTargetPosition = null;
            return;
        }

        // Get current position
        _currentTarget.TryGetComponent<Positioned>(out var entityPos);
        if (entityPos == null)
            return;

        var currentPos = entityPos.GridPos;

        // Check if target has moved and if we still have line of sight
        if (currentPos != _currentTargetPosition)
        {
            var rawPathfindingData = GameController.IngameState.Data.RawPathfindingData;
            if (rawPathfindingData == null)
                return;

            var playerPos = GameController.Game.IngameState.Data.LocalPlayer.GridPos;
            bool hasLineOfSight = HasLineOfSight(
                (int)playerPos.X, (int)playerPos.Y,
                (int)currentPos.X, (int)currentPos.Y,
                rawPathfindingData
            );

            if (!hasLineOfSight)
            {
                _currentTarget = null;
                _currentTargetPosition = null;
                return;
            }

            _currentTargetPosition = currentPos;
        }

        // Move cursor to target's screen position
        var screenPos = GameController.Game.IngameState.Data.GetGridScreenPosition(currentPos);
        if (screenPos != Vector2.Zero)
        {
            Input.SetCursorPos(screenPos);
        }
    }

    private Vector2? FindNearestMonsterPositionOld()
    {
        if (GameController?.EntityListWrapper?.ValidEntitiesByType == null)
            return null;

        Entity nearestMonster = null;
        float nearestDistance = float.MaxValue;

        // Find nearest hostile monster
        var monsters = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster] ?? Enumerable.Empty<Entity>();
        foreach (var monster in monsters)
        {
            if (monster == null || !monster.IsHostile || !monster.IsAlive) 
                continue;

            var distance = monster.DistancePlayer;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestMonster = monster;
            }
        }

        if (nearestMonster != null)
        {
            return nearestMonster.GridPos;
        }

        return null;
    }

    private List<Vector2> GetPathToCursor()
    {
        var path = new List<Vector2>();
        if (!Settings.Enable.Value || !Settings.Pathfinding.EnablePathfinding.Value || 
            GameController?.IngameState?.Data == null || GameController?.Game?.IngameState?.Data?.LocalPlayer == null)
            return path;

        var playerGridPos = GameController.Game.IngameState.Data.LocalPlayer.GridPos;
        
        // Get nearest monster position instead of cursor position
        var targetPos = FindNearestMonsterPosition();
        if (targetPos == null)
            return path;

        // Get the raw pathfinding data from the game
        var rawPathfindingData = GameController.IngameState.Data.RawPathfindingData;
        if (rawPathfindingData == null)
            return path;

        // Convert target position to grid coordinates
        var targetGridX = (int)targetPos.Value.X;
        var targetGridY = (int)targetPos.Value.Y;
        var playerGridX = (int)playerGridPos.X;
        var playerGridY = (int)playerGridPos.Y;

        // Ensure coordinates are within bounds
        if (targetGridX < 0 || targetGridY < 0 || playerGridX < 0 || playerGridY < 0 ||
            targetGridX >= rawPathfindingData[0].Length || targetGridY >= rawPathfindingData.Length ||
            playerGridX >= rawPathfindingData[0].Length || playerGridY >= rawPathfindingData.Length)
            return path;

        // Check if start and end points are walkable
        if (rawPathfindingData[playerGridY][playerGridX] == 0 || 
            rawPathfindingData[targetGridY][targetGridX] == 0)
            return path;

        // Initialize pathfinder with the game's pathfinding data
        var pathFinder = new PathFinder(rawPathfindingData, new[] { 1, 2, 3, 4, 5 }); // Use all walkable terrain types

        var pathPoints = pathFinder.FindPath(
            new GameOffsets2.Native.Vector2i(playerGridX, playerGridY),
            new GameOffsets2.Native.Vector2i(targetGridX, targetGridY)
        );

        if (pathPoints != null)
        {
            // Convert grid coordinates back to world positions
            path.Add(playerGridPos); // Start with player position
            foreach (var point in pathPoints)
            {
                path.Add(new Vector2(point.X, point.Y));
            }
            
            // Add the exact target position as the final point
            path.Add(targetPos.Value);
        }

        return path;
    }

    private void DrawPathLines(List<Vector2> pathPoints, Color color, float lineThickness)
    {
        if (pathPoints.Count < 2) return;
        if (GameController?.Window == null || RemoteMemoryObject.TheGame?.IngameState?.Camera == null) return;

        var screensize = new RectangleF
        {
            X = 0,
            Y = 0,
            Width = GameController.Window.GetWindowRectangleTimeCache.Size.X,
            Height = GameController.Window.GetWindowRectangleTimeCache.Size.Y
        };

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            try
            {
                var startPos = QueryGridPositionToWorldWithTerrainHeight(pathPoints[i]);
                var endPos = QueryGridPositionToWorldWithTerrainHeight(pathPoints[i + 1]);

                var startScreenPos = RemoteMemoryObject.TheGame.IngameState.Camera.WorldToScreen(startPos);
                var endScreenPos = RemoteMemoryObject.TheGame.IngameState.Camera.WorldToScreen(endPos);

                if (IsEntityWithinScreen(startScreenPos, screensize, 50) || 
                    IsEntityWithinScreen(endScreenPos, screensize, 50))
                {
                    Graphics.DrawLine(startScreenPos, endScreenPos, lineThickness, color);
                }
            }
            catch (Exception)
            {
                // Ignore any drawing errors and continue with the next line segment
                continue;
            }
        }
    }
}