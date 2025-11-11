# Procedural Map Generation

## Ressources

Ce projet utilise 2 packages externes à Unity :
- **Architecture Procedural Generation**
- **Unitask**

Installation via OpenUPM : https://openupm.com/packages/com.cysharp.unitask/#modal-manualinstallation

## Getting Started

### Architecture Procedural Generation

Le package **Architecture Procedural Generation** possède déjà un système complet de génération de noise map avec plusieurs paramètres configurables tels que :
- Octaves
- Fréquence
- Type de noise
- Etc.

#### ProceduralGenerationMethod

Il inclut sa propre méthode `ProceduralGenerationMethod` qui permet de créer de nouveaux types de méthodes de génération procédurale à utiliser dans le script **Procedural Grid Generator**.

#### Gestion des Tiles

Le package possède ses propres fonctions permettant de générer une Grid et des tiles de différentes couleurs selon le nom de la tile ou du ScriptableObject :
```csharp
ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Name");

protected const string ROOM_TILE_NAME = "Room";
protected const string CORRIDOR_TILE_NAME = "Corridor";
protected const string GRASS_TILE_NAME = "Grass";
protected const string WATER_TILE_NAME = "Water";
protected const string ROCK_TILE_NAME = "Rock";
protected const string SAND_TILE_NAME = "Sand";
```

#### Grid

La classe **Grid** permet de :
- Récupérer sa taille
- Récupérer et créer une cellule valide de la grid
```csharp
if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
{
    Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
    continue;
}
```

#### GridGenerator

La classe **GridGenerator** permet de placer une tile sur une cellule :
```csharp
AddGridObjectToCell(Cell cell, GridObjectTemplate template, bool overrideExistingObjects);
// ou
AddTileToCell(Cell cell, string tileName, bool overrideExistingObjects); // équivalent
```

## Simple Room Placement

Cette algo permet de créer des salle rectangulaire de differente taille et de les relier par des chemins/couloirs
![Map Generation Example](images/map-simpleroom.png)

### Génération des salles
La logique est plutot simple, on prend un point random dans la grid et une taille de romm random 
```csharp
private RectInt GetRandomRoom()
{
    int xPos = RandomService.Range(0, Grid.Width);
    int yPos = RandomService.Range(0, Grid.Lenght);

    int sizeX = RandomService.Range(2, 10);
    int sizeY = RandomService.Range(2, 10);

    RectInt room = new RectInt(xPos, yPos, sizeX, sizeY);
    return room;
}
```
On parcourt l'intégralité de notre Grid en verifiant bien que la Cell ne contient pas deja quelque chose via containObject
```csharp
for (int x = room.xMin - 1; x <= room.xMax + 1; x++)
{
    for (int y = room.yMin - 1; y <= room.yMax + 1; y++)
    {
        if (Grid.TryGetCellByCoordinates(x, y, out Cell cell))
        {
            if (cell.ContainObject)
                return;
        }
    }
}
```
puis on re-parcourt la Grid mais cette fois ci pour poser notre tile de la room et enfin une fois la double boucle finis, on ajoute la room à notre liste de room pour pouvoir faire les couloirs après

### Placement des couloirs
Pour relier les salles entre elles, on parcourt notre liste de rooms et on connecte chaque salle à la suivante en créant un chemin en forme de "L".

La méthode calcule d'abord le centre de chaque salle, puis trace un couloir horizontal jusqu'à atteindre la coordonnée X de la salle cible, et enfin un couloir vertical pour rejoindre la coordonnée Y de destination.
```csharp
private void PlaceCorridors()
{
    for(int i = 0; i < roomList.Count - 1; i++)
    {
        Vector2Int roomStart = roomList[i].GetCenter();
        Vector2Int roomTarget = roomList[i + 1].GetCenter();
        
        int xDir = roomTarget.x > roomStart.x ? 1 : -1;
        int yDir = roomTarget.y > roomStart.y ? 1 : -1;
        
        // Trace le couloir horizontal
        while (roomStart.x != roomTarget.x)
        {
            if(Grid.TryGetCellByCoordinates(roomStart.x, roomStart.y, out Cell cell))
            {
                AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
            }
            roomStart.x += xDir;
        }
        
        // Trace le couloir vertical
        while (roomStart.y != roomTarget.y)
        {
            if (Grid.TryGetCellByCoordinates(roomStart.x, roomStart.y, out Cell cell))
            {
                AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
            }
            roomStart.y += yDir;
        }
    }
}
```

Le paramètre `overrideExistingObjects` est défini sur `false` pour éviter d'écraser les tiles de room déjà placées lors du tracé des couloirs.
