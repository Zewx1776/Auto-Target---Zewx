# WhereAreYouGoing Plugin

A Path of Exile plugin that implements intelligent pathfinding and cursor movement to enhance gameplay navigation.

## Features

### Advanced Pathfinding

The plugin uses a sophisticated pathfinding system combining two powerful algorithms:

1. **A* (A-Star) Algorithm**
   - Efficiently finds the optimal path between two points
   - Uses heuristic distance estimation to prioritize promising paths
   - Maintains a balance between path length and computational efficiency
   - Implemented using a binary heap for optimal performance

2. **Branheimns Algorithm for Cursor Movement**
   - Smooth cursor movement along calculated paths
   - Dynamic speed adjustment based on distance
   - Precise targeting for both nearby and distant locations

### Key Components

#### Pathfinding System
- Grid-based navigation using game's terrain data
- Obstacle avoidance and terrain consideration
- Efficient path recalculation when needed
- Binary heap implementation for optimal path selection

#### Cursor Movement
- Automated cursor positioning
- Smooth interpolation between points
- Line-of-sight checking
- Dynamic speed adjustment

## Technical Implementation

### PathFinder Class
The core pathfinding implementation uses a grid-based system that:
- Processes terrain data to determine walkable areas
- Implements A* algorithm for optimal path calculation
- Uses efficient data structures (binary heap) for performance
- Handles edge cases and invalid paths

### Movement Control
- Precise cursor control using game coordinates
- Terrain height consideration for accurate positioning
- Dynamic path updates based on game state changes

## Usage

1. Enable the plugin through the ExileCore interface
2. Configure pathfinding settings as needed
3. The plugin will automatically handle pathfinding and cursor movement

## Performance

The implementation focuses on efficiency through:
- Optimized A* implementation with binary heap
- Smart caching of calculated paths
- Efficient grid traversal algorithms
- Minimal memory footprint