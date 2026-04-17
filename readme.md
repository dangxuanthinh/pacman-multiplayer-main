Pacman Multiplayer Project Notes:

Since this is a multiplayer game, you’ll need to run multiple builds at the same time to play multiplayer. You can still play in single-player mode by creating a lobby and playing normally. To run multiple instances, you can either open several builds at once or share the build with another person so you can play together (playing with others is recommended).

To run multiple instances:

* You can run one build in the Unity Editor, and another by launching the Pac Adventure.exe file.
* Or you can open Pac Adventure.exe multiple times to run several game windows simultaneously.

Game Flow:

When you start the game, you’ll enter the MainMenu scene, which has 3 tabs: Achievements, Multiplayer, Options.

Achievements Tab:
Displays all achievements in the game. When you meet the required conditions during gameplay, the achievement will be marked as completed.

Options Tab:
Contains 2 sliders to adjust the game’s volume.

Multiplayer Tab:
Opens a Login window. After creating an account and logging in, you’ll be taken to the Lobby scene. In the Lobby, players can create a room or join an existing one.

Room Settings:

When creating a room, you can:

* Set the room name
* Choose a map
* Set the room to public or private
* Select game mode: Classic or Survival
* Adjust AI difficulty (higher difficulty makes the 4 ghosts more aggressive and faster)
  If you're new to Pacman, it's recommended to set AI to Easy for testing.

After creating or joining a room, you’ll enter the CharacterSelect scene:

* You can customize your character model
* Other players in the room will also appear here
* When all players press Ready, the host can press Start Game to begin

Important Note:
If the host disconnects or closes the game while in the CharacterSelect scene or during gameplay, all connected clients will be disconnected.

Controls:

* Move using Arrow Keys or WASD

Gameplay Objective:

* Eat as many pellets and power-ups as possible
* Avoid being caught by the 4 ghosts

Game Modes:

Classic Mode:
Eat all pellets on the map to win.

Survival Mode:
Similar to Classic, but you must survive for 4 minutes to win.
Occasionally, a strawberry will spawn in the center of the map. When collected, it respawns all previously eaten pellets.

Lives System:

* Each player has 3 lives
* Getting caught by ghosts 3 times results in Game Over

Achievements During Gameplay:
If a player meets the conditions for an achievement while playing, a UI popup will appear indicating the achievement is completed.
