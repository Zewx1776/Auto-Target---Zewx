using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using System.Drawing;

namespace WhereAreYouGoing
{
    public class WhereAreYouGoingSettings : ISettings
    {
        public enum UnitType
        {
            None,
            Normal,
            Magic,
            Rare,
            Unique,
            Self,
            Friendly,
            Player,
            UnitTesting
        }

        public ToggleNode Enable { get; set; } = new ToggleNode(false);
        public ToggleNode MultiThreading { get; set; } = new ToggleNode(false);
        public RangeNode<int> MaxCircleDrawDistance { get; set; } = new RangeNode<int>(120, 0, 200);
        public HotkeyNode TargetNearestEnemyKey { get; set; } = new HotkeyNode(System.Windows.Forms.Keys.F);
        public RangeNode<int> MaxTargetRange { get; set; } = new RangeNode<int>(200, 0, 500);

        // Pathfinding Settings
        public class PathfindingSettings
        {
            public ToggleNode EnablePathfinding { get; set; } = new ToggleNode(true);
            public ColorNode PathColor { get; set; } = new ColorNode(Color.Yellow);
            public RangeNode<float> PathThickness { get; set; } = new RangeNode<float>(2.0f, 1.0f, 10.0f);
        }

        public PathfindingSettings Pathfinding { get; set; } = new PathfindingSettings();

        public WAYGConfig NormalMonster { get; set; } = new WAYGConfig()
        {
            Enable = true,
            UnitType = UnitType.Normal,
            Colors = new WAYGConfig.WAYGColors
            {
                MapColor = Color.FromArgb(94, 255, 255, 255),
                MapAttackColor = Color.FromArgb(255, 255, 0, 0),
                WorldColor = Color.FromArgb(94, 255, 255, 255),
                WorldAttackColor = Color.FromArgb(255, 255, 0, 0),
            },
            World = new WAYGConfig.WAYGWorld
            {
                Enable = true,
                DrawAttack = false,
                DrawAttackEndPoint = true,
                DrawDestination = true,
                DrawDestinationEndPoint = false,
                DrawLine = false,
                AlwaysRenderWorldUnit = true,
                DrawFilledCircle = false,
                DrawBoundingBox = false,
                RenderCircleThickness = 3,
                LineThickness = 5
            },
            Map = new WAYGConfig.WAYGMap
            {
                Enable = false,
                DrawAttack = true,
                DrawDestination = true,
                LineThickness = 1
            }
        };

        public WAYGConfig MagicMonster { get; set; } = new WAYGConfig()
        {
            Enable = true,
            UnitType = UnitType.Magic,
            Colors = new WAYGConfig.WAYGColors
            {
                MapColor = Color.FromArgb(176, 43, 120, 255),
                MapAttackColor = Color.FromArgb(144, 255, 0, 0),
                WorldColor = Color.FromArgb(176, 43, 120, 255),
                WorldAttackColor = Color.FromArgb(144, 255, 0, 0),
            },
            World = new WAYGConfig.WAYGWorld
            {
                Enable = true,
                DrawAttack = false,
                DrawAttackEndPoint = true,
                DrawDestination = true,
                DrawDestinationEndPoint = false,
                DrawLine = false,
                AlwaysRenderWorldUnit = true,
                DrawFilledCircle = false,
                DrawBoundingBox = false,
                RenderCircleThickness = 3,
                LineThickness = 5
            },
            Map = new WAYGConfig.WAYGMap
            {
                Enable = false,
                DrawAttack = true,
                DrawDestination = true,
                LineThickness = 1
            }
        };

        public WAYGConfig RareMonster { get; set; } = new WAYGConfig()
        {
            Enable = true,
            UnitType = UnitType.Rare,
            Colors = new WAYGConfig.WAYGColors
            {
                MapColor = Color.FromArgb(255, 225, 210, 19),
                MapAttackColor = Color.FromArgb(140, 255, 0, 0),
                WorldColor = Color.FromArgb(255, 225, 210, 19),
                WorldAttackColor = Color.FromArgb(140, 255, 0, 0),
            },
            World = new WAYGConfig.WAYGWorld
            {
                Enable = true,
                DrawAttack = true,
                DrawAttackEndPoint = true,
                DrawDestination = true,
                DrawDestinationEndPoint = true,
                DrawLine = true,
                AlwaysRenderWorldUnit = true,
                DrawFilledCircle = true,
                DrawBoundingBox = false,
                RenderCircleThickness = 5,
                LineThickness = 5
            },
            Map = new WAYGConfig.WAYGMap
            {
                Enable = false,
                DrawAttack = true,
                DrawDestination = true,
                LineThickness = 5
            }
        };

        public WAYGConfig UniqueMonster { get; set; } = new WAYGConfig()
        {
            Enable = true,
            UnitType = UnitType.Unique,
            Colors = new WAYGConfig.WAYGColors
            {
                MapColor = Color.FromArgb(255, 226, 122, 33),
                MapAttackColor = Color.FromArgb(255, 255, 0, 0),
                WorldColor = Color.FromArgb(255, 226, 122, 33),
                WorldAttackColor = Color.FromArgb(255, 255, 0, 0),
            },
            World = new WAYGConfig.WAYGWorld
            {
                Enable = true,
                DrawAttack = true,
                DrawAttackEndPoint = true,
                DrawDestination = true,
                DrawDestinationEndPoint = true,
                DrawLine = true,
                AlwaysRenderWorldUnit = true,
                DrawFilledCircle = true,
                DrawBoundingBox = false,
                RenderCircleThickness = 5,
                LineThickness = 5
            },
            Map = new WAYGConfig.WAYGMap
            {
                Enable = true,
                DrawAttack = true,
                DrawDestination = true,
                LineThickness = 3
            }
        };

        public WAYGConfig TestingUnits { get; set; } = new WAYGConfig()
        {
            Enable = true,
            UnitType = UnitType.UnitTesting,
            Colors = new WAYGConfig.WAYGColors
            {
                MapColor = Color.FromArgb(255, 226, 122, 33),
                MapAttackColor = Color.FromArgb(255, 255, 0, 0),
                WorldColor = Color.FromArgb(255, 226, 122, 33),
                WorldAttackColor = Color.FromArgb(255, 255, 0, 0),
            },
            World = new WAYGConfig.WAYGWorld
            {
                Enable = true,
                DrawAttack = true,
                DrawAttackEndPoint = true,
                DrawDestination = true,
                DrawDestinationEndPoint = true,
                DrawLine = true,
                AlwaysRenderWorldUnit = true,
                DrawFilledCircle = true,
                DrawBoundingBox = false,
                RenderCircleThickness = 5,
                LineThickness = 5
            },
            Map = new WAYGConfig.WAYGMap
            {
                Enable = true,
                DrawAttack = true,
                DrawDestination = true,
                LineThickness = 3
            }
        };

        public WAYGConfig Self { get; set; } = new WAYGConfig()
        {
            Enable = true,
            UnitType = UnitType.Self,
            Colors = new WAYGConfig.WAYGColors
            {
                MapColor = Color.FromArgb(193, 35, 194, 47),
                MapAttackColor = Color.FromArgb(255, 255, 0, 0),
                WorldColor = Color.FromArgb(193, 35, 194, 47),
                WorldAttackColor = Color.FromArgb(255, 255, 0, 0),
            },
            World = new WAYGConfig.WAYGWorld
            {
                Enable = true,
                DrawAttack = true,
                DrawAttackEndPoint = true,
                DrawDestination = true,
                DrawDestinationEndPoint = true,
                DrawLine = true,
                AlwaysRenderWorldUnit = true,
                DrawBoundingBox = false,
                RenderCircleThickness = 3,
                LineThickness = 6
            },
            Map = new WAYGConfig.WAYGMap
            {
                Enable = true,
                DrawAttack = true,
                DrawDestination = true,
                LineThickness = 5
            }
        };

        public WAYGConfig Players { get; set; } = new WAYGConfig()
        {
            Enable = true,
            UnitType = UnitType.Player,
            Colors = new WAYGConfig.WAYGColors
            {
                MapColor = Color.FromArgb(193, 35, 194, 47),
                MapAttackColor = Color.FromArgb(255, 255, 0, 0),
                WorldColor = Color.FromArgb(193, 35, 194, 47),
                WorldAttackColor = Color.FromArgb(255, 255, 0, 0),
            },
            World = new WAYGConfig.WAYGWorld
            {
                Enable = true,
                DrawAttack = true,
                DrawAttackEndPoint = true,
                DrawDestination = true,
                DrawDestinationEndPoint = true,
                DrawLine = true,
                AlwaysRenderWorldUnit = true,
                DrawBoundingBox = false,
                RenderCircleThickness = 3,
                LineThickness = 6
            },
            Map = new WAYGConfig.WAYGMap
            {
                Enable = true,
                DrawAttack = true,
                DrawDestination = true,
                LineThickness = 5
            }
        };

        public WAYGConfig Minions { get; set; } = new WAYGConfig()
        {
            Enable = true,
            UnitType = UnitType.Friendly,
            Colors = new WAYGConfig.WAYGColors
            {
                MapColor = Color.FromArgb(255, 218, 73, 255),
                MapAttackColor = Color.FromArgb(121, 255, 73, 115),
                WorldColor = Color.FromArgb(255, 218, 73, 255),
                WorldAttackColor = Color.FromArgb(121, 255, 73, 115),
            },
            World = new WAYGConfig.WAYGWorld
            {
                Enable = true,
                DrawAttack = true,
                DrawAttackEndPoint = true,
                DrawDestination = true,
                DrawDestinationEndPoint = true,
                DrawLine = true,
                AlwaysRenderWorldUnit = true,
                DrawBoundingBox = false,
                RenderCircleThickness = 5,
                LineThickness = 5
            },
            Map = new WAYGConfig.WAYGMap
            {
                Enable = false,
                DrawAttack = true,
                DrawDestination = true,
                LineThickness = 5
            }
        };
    }
}