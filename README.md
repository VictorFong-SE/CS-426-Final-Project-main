# CS-426-Final-Project

## Controls

Left Mouse Button - Attacks the enemy, which staggers them if they’re not blocking or attacking (blank moves). Attacking when enemy attacks results in taking damage

Right Mouse Button - Blocks enemy attacks, can be done anytime, and anywhere

Space - Kicks enemy, acts the same as Attack, but functions separately within combos

Mouse Movement - Move’s player view/rotate the player

WASD - Standard first-person movement (forward, left, back, right)

Q/E - Whether player is using their spells or sword respectively

F - Interact (Open/Close Door)

Tab - Toggle mouse capture

## Assignment 7

### Design Rational

Our game, Sir Metronome, is a combination of formal elements from rhythm games, adventure/puzzle games, and 3rd Person combat. In order to better achieve our design goal we needed to implement the various requirements for this assignment and then some. The physics constructs were needed in order to create the particle system for the fire spell, do collision and movement for the player via colliders and rigid bodies, and hinge joints to make the doors throughout the level usable. In order to detail the level and make it look more appealing we needed texures and lighting. The lighting we did (various point lights on the torches, the fire spell glowing, and the mana barrel glowing as well) all added to the feel and immersion of the level, and add to the fantasy/medeival theme. The textures helped stylize the level and add the the cartoony/low poly design we were going for. The animations from mixamo, the mecanim systems we developed for the player, minion, and boss, and the sounds for each state all give great feedback to the player about the state of the game, which keeps them engaged. Finally the AI systems were the most important part of the game because they make the game actually playable. The various managers, directors, Boss and minon AI types, and pathfinding work together to make the intial game idea a reality by balancing fun with difficulty, as well as adding varied play.

### Mecanim Constructs

Jonathan Laughton:

- Player Animations
  - Movement (2D Freeform Blend Tree):
    - 2 run animations (forward, and backward)
    - 2 strafe animation (right, and left)
    - 1 idle animation
  - Moves:
    - Attack
    - Block
    - Kick
    - Spell Cast
    - Draw Sword
    - Sheath Sword

Zain Aamer:

- Minion Animations:
  - Engaged:
    - Movement (2D Freeform Blend Tree):
      - 3 long steps (forward, left, right)
      - 4 medium steps (forward, left, right, backward)
      - 3 short steps (forward, left, right)
    - Moves:
      - Attack
      - Block:
        - Ordinary block
        - Block and get hit
    - Engaged Idle
  - Not engaged:
    - Movement (2D Freeform Blend Tree):
      - 8 jogging (forward, left, right, backward, forward-left, forward-right, backward-left, backward-right)
      - 4 walkings (forward, left, right, backward)
      - 2 strafe (left, right)
        - Faster than walking, slower than jogging
    - Not Engaged Idle
  - Transitions:
    - Not Engaged Idle to Engaged Idle
    - Engaged Idle to Not Engaged Idle
  - React to hit
  - Dying

Victor Fong:

- Boss Animations:
  - Moves:
    - Attack
    - Block
    - Idle
    - Stun
  - Death

### AI Constructs

Jonathan Laughton:

- Player Move Prediction in Boss AI:
  - Construct: Search (Minimax)
  - Predicts player moves based current precalculated boss moves, and then based on the boss's possible respones to each player response
  - The boss chooses the move that leads to the least amount of damage assuming optimal player play
  - When depth of recursion is reached it evaluates moves based on the possible combos it predicted and determines the damage for that set of moves
    - For all possible ranges of the player moves we calculated we add them to inprogress combos and check for a valid combo
      - If valid we select that combo otherwise selected is null
    - If selected was found we check each sub list from 0 to i with i going to the size of selected, and check for valid combos
      - If it's valid then we decrement score by the combo damage and clear our temp sub list
    - Then we determine if we have any idle moves and if the player did an attack move (kick or attack) for all predicted moves
      - If we did then we decrement score by 1

Zain Aamer:

- Player Move Prediction in Boss AI:
  - Construct: Bayesian network
  - Tracks player moves during and after stunning a boss
  - If the player had 2 moves left of a 5-move combo:
    - The boss chooses 2 moves that allow the player to finish the combo
  - If the player cannot finish a 5-move combo:
    - The Bayesian network uses their previous three moves to predict their fourth.
  - If the predicted fourth move is going to complete a 4 combo:
    - The boss chooses a move to allow it.
  - Otherwise:
    - The boss will choose a move to minimize any benefit from performing the predicted fourth move, to incentivize combos.

- Navigation:
  - Construct: Navmesh & A*
  - Generated a navmesh for the map for the given minion size
  - Carves navmesh for doors when opened and closed
    - Allows enemies to follow if close behind, but otherwise they stay behind.
  - Spaces out minions so that they don't cluster

<details>
<summary>Other AI, not constructs from slides, but here for context with the rest</summary>
<br>
  
- Boss AI Director:
  - Two-states for managing the boss AI playability.
  - States:
    - Improv
      - The boss uses their own specific logic to choose a move
        - Ex: Randomizing moves, copying player's, calculating one via minimax, etc.
      - The boss transitions to the Directed state when stamina hits 0
        - Def: Stamina is a countdown since the last IDLE was chosen
    - Directed
      - The boss defers to a series of shared rules
        - Makes sure there is an IDLE at least once every N (5 by default) turns
        - Stuns the boss if the player attacks on IDLE and they're not already stunned
        - Uses the Bayesian network and combo tracker to decide moves as described in the Bayesian network section after stun ends
      - Once it has fulfilled its scripted moves, it returns to the Improv state.

- Minion AI and Director:
  - Two main states, with one having 3 substates
  - States:
    - Not Engaged
      - Not fighting the player
      - Stays nearby
    - Engaged (Corresponds to enemy moves)
      - IDLE
      - ATTACK
      - BLOCK
  - AI director decides which states are allowed
    - Ex: No ATTACK when another could be in IDLE
  - The minion requests a token from the blackboard as permission to transition to that state.
  - The blackboard uses a token acquisition system, inspired by the one used in Doom 2016
    - Limit the number of engaged enemies
    - Prioritize those deemed more important
      - Ex: Closer to the player
    - Control state transitions
  - A minion requests the desired token
    - Ex: Attack token for attack state
  - If denied continues down to the next desired state, and so forth.

</details>

Victor Fong:

- AISwitcher is an FSM AI handler that instantiates and manages a created FSM with states representing the three main AI the enemy Boss uses to combat the player during combat. AISwitcher is passed the boss object each update in order to evaluate it's health and determine which state to switch to (if a switch is necessary at all), and defaults to StateRandom at initialization.

- StateMachine is the FSM hub which initiates and begins the FSM. Each state is a single instance instantiated as it is switched to, ensuring clean usage and memory optimization. The StateMachine status is checked by AISwitcher every beat.
  - StateRandom sets the Boss AI to use the RandomAI.
  - StateCopyCat sets the Boss AI to use the CopyCatAI.
  - StateMiniMax sets the Boss AI to use the MiniMaxAI.

## Assignment 8

### Overall Design Changes

- All NPCs will attack when player does, if player does. Functions the same as before, just looks and feels better.
- Dart trap removal, as the game is already hard enough as-is.
- Changing enemy placements so there are more later, in the more open areas.

### UI Design

- Recolored health bars
- Add indicators for:
  - Boss stages
  - Spell cooldown
  - Locked doors
- Redid boss move indicator to use symbols
- Added highlights to powerups
- On-beat versus offbeat feedback in the indicator
- "Hit marker" for spells

### Sound Design

- Made various 2D sounds, 3D
- Add variety for existing attack sounds
- Make them differ based on hits and misses
- Added:
  - Boss music
  - Minion combat music
  - Ambient sounds
  - Blending between combat and ambience
- Door opening, level and mana barrel sounds.

## Assignment 9


### In Response to Alpha Feedback

For one, we increased the window for moves from 0.35 seconds to 0.4 seconds, to make it easier, since it was too difficult for the testers to give meaningful feedback on combat.
We increased the light range and added torches on the boss door to make it very well lit.
We constrained the camera movement substantially, so it is always looking forward, as that was told to us by the testers as the main issue they had.
We moved the boss move GUI element to the top right corner, so it is closer to the flashing beat element. This way the player doesn’t have to keep looking up and down in the middle of a beat.
Since assignment 8 required at least 2 variations of each event’s sounds, it was harder for the testers to identify the player’s attack versus the minions’. However, that is a requirement, so we can’t resolve that unfortunately. Instead we made landing a hit do half damage on a block, so the player doesn’t feel confused, since they’ll always see a health change on hit. This involved doubling all other damage and health, since those were ints and we needed a fine unit of health.
We also added a controls screen and how to play screen, as that seemed to be the hardest part for testers to grasp. Depending on feedback on the Beta test, we may adjust the wording or explanation.

### Shader Design

#### Jonathan Laughton

- Mana Liquid Shader: This shader is a slightly modified toon water shader to give it a more blueish-purple hue. It uses the depth texture from the unity camera to figure out which color the liquid should be and uses noise to create ripple effects. This shader was added to make the mana barrel appear more dynamic instead of just being a static texture. It’s intended to give a higher sense of immersion.

#### Zain Aamer

- Interactable glow: This shader is a fairly modified outline shader, tweaked for both shading needs in terms of color and to allow smooth transparency. It glows, giving a white outline to interactable objects. We then adjust the alpha of the glow, depending on the player’s distance to the object in question. This makes it a lot clearer when something is interactable. While noticeable, it’s not too over the top, and doesn’t take away from the overall aesthetic, so I think it’s a nice addition.

#### Victor Fong

- Text Culling: This shader implements culling in order to prevent certain texts from appearing over in game objects. Most notably, this prevents text from showing over crates, walls, etc when Minions are preparing their next move. Which increases the overall look and feel of the game.

### Writing

We added both the story text, elaborated in the story section at the beginning of this document, as well as credits available from the main menu or upon completing the game.


## Credits

POLYGON Adventure Pack (characters, and detailing) - Synty Studios - <https://assetstore.unity.com/packages/3d/environments/fantasy/polygon-adventure-pack-80585>

Low Poly Dungeon Lite (detailing and lighting) - JustCreate - <https://assetstore.unity.com/packages/3d/environments/dungeons/low-poly-dungeons-lite-177937>

Unique Toon Projectiles - Gabriel Aguiar Productions - <https://assetstore.unity.com/packages/vfx/particles/unique-toon-projectiles-vol-1-139417>

Hand Painted Stone Texture - Lowlypoly - <https://assetstore.unity.com/packages/2d/textures-materials/floors/hand-painted-stone-texture-73949>

RPG Pack (potion bottle) - Alexander Kotov - <https://assetstore.unity.com/packages/3d/props/potions-coin-and-box-of-pandora-pack-71778>

StoneWalls Normal Maps - BigMiniGeek - <https://assetstore.unity.com/packages/3d/stonewalls-normal-maps-64841>

Interactive Physical Door Pack - Art Notes - <https://assetstore.unity.com/packages/tools/physics/interactive-physical-door-pack-163249>

Shikashi's Fantasy Icons Pack - Matt Firth (cheekyinkling) - <https://cheekyinkling.itch.io/shikashis-fantasy-icons-pack>

Simple Wooden Barrel Pack - Surpent - <https://assetstore.unity.com/packages/3d/props/simple-wooden-barrel-pack-18994>

FREE Medieval Structure Kit - Ferocious Industries - <https://assetstore.unity.com/packages/3d/environments/fantasy/free-medieval-structure-kit-141700>

Low Poly Ultimate Pack - polyperfect - <https://assetstore.unity.com/packages/3d/props/low-poly-ultimate-pack-54733>

Five Seamless Tileable Ground Textures (texturing the mud) - A3D - <https://assetstore.unity.com/packages/2d/textures-materials/floors/five-seamless-tileable-ground-textures-57060>

Dungeon Stone Texutures - 3d.rina - <https://assetstore.unity.com/packages/2d/textures-materials/stone/dungeon-stone-textures-66487>

Cinematic Punch, Kicks, Blocks: Sound Effect Pack (Minion Audio) - Classic Sounds - <https://www.youtube.com/watch?v=NZvCnpYdHqE>

Sword Sound - dermote - <https://freesound.org/people/dermotte/sounds/263011/>

Blows through the air whoosh sound - garuda1982 - <https://freesound.org/people/Garuda1982/sounds/538912/>

Male Pain/Hurt Sound Effects - jocabundus - <https://www.youtube.com/watch?v=7GDq2MKjyec>

Success - grunz - <https://freesound.org/people/grunz/sounds/109662/>

Kick - sfxbuzz - <https://www.sfxbuzz.com/summary/7-free-fight-sounds-sound-effects/95-kick-fighting-sound>

Battle Of The Creek - Alexander Nakarada - <https://www.free-stock-music.com/alexander-nakarada-battle-of-the-creek.html>

The sounds of punching - kretopi - <https://freesound.org/people/kretopi/sounds/406460/>

Strong Melee Swing - SypherZent - <https://freesound.org/people/SypherZent/sounds/420670/>

Punch/Kick Being Blocked - elynch0901 - <https://freesound.org/people/elynch0901/sounds/464498/>

Slash - qubodup - <https://freesound.org/people/qubodup/sounds/442903/>

slashkut.wav - Abyssmal - <https://freesound.org/people/Abyssmal/sounds/35213/>

Whoosh - qubodup - <https://freesound.org/people/qubodup/sounds/60013/>

Bubble sounds (mana barrel) - audiolarx - <https://freesound.org/people/audiolarx/sounds/263945/>

Creepy Metal Door Bang - qubodup - <https://freesound.org/people/qubodup/sounds/448947/>

Pop 2 - greenvwbeetle - <https://freesound.org/people/greenvwbeetle/sounds/244654/>

Adventure | Royalty Free Medieval Fantasy Music - Alexander Nakarada  - <https://www.youtube.com/watch?v=7_cwKd81z7Q>

Walk on Tile Sound Effects - SFX Box - <https://www.youtube.com/watch?v=rF-ArLYzlJU>

Clank1.wav - BMacZero - <https://freesound.org/people/BMacZero/sounds/94127/>

whoosh 2 - Lukas Eriksen - <https://www.youtube.com/watch?v=iPToKmyZi74>

kick 2 - HDSoundEffects - <https://www.youtube.com/watch?v=f-fMPjIp3Bc>

Spell Cooldown image/spritesheets - Pipoya - <https://pipoya.itch.io/pipoya-free-vfx-time-magic>

Text Culling Shader Tutorial - Eric Haines (Eric5h5) - https://wiki.unity3d.com/index.php?title=3DText

Arial font family - DownloadFonts - https://www.downloadfonts.io/arial-font-family-free/

Mana Barrel Shader Tutorial - Roystan - <https://roystan.net/articles/toon-water.html>

Silhouette-Outlined Diffuse - AnomalousUnderdog - <http://wiki.unity3d.com/index.php?title=Silhouette-Outlined_Diffuse>

Pixel-Perfect Outline Shaders for Unity - Doug Valenta - <https://www.videopoetics.com/tutorials/pixel-perfect-outline-shaders-unity>

EB-Garamond - Georg Duffner and Octavio Pardo - <https://hyperpix.net/fonts/dark-souls-font/#:~:text=%E2%80%9CAdobe%20Garamond%20Bold%E2%80%9D%20is%20the,Granjon%20and%20published%20by%20Adobe.>
