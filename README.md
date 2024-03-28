# EConfig Management Tools

EConfig Management Tools is a set of Unity Editor extensions that streamline the management and organization of ScriptableObjects representing configurations or settings in your game development workflow.

![EConfigManager Window](/EConfigManager.png)

## Features

- **EConfigHolder**: Centralized management of your project configurations.
- **Select Configs Window**: A user-friendly interface for selecting and managing ScriptableObject configurations.
- **Drag-and-Drop**: Intuitive addition of new configurations.
- **Sorting**: Organize your configurations by name or date modified.

## Installation

Copy the `EConfigManager` folder into your Unity project's Editor folder.

## Usage

Open the `EConfigManager` window through Unity's top menu `Escripts > EConfigManager`.

### Config Selection and Management

- Use the Select Configs window to view and organize your configurations.
- Add new configurations by dragging them into the list or using the ALL button to include every scriptable in your project.
- Remove configurations with the remove button.

### Saving and Asset Management

All changes will be saved immediately to the Unity Asset Database.
The Clean List button allows you to clear the current list of configurations, and the ALL button populates the list with all available ScriptableObjects.
