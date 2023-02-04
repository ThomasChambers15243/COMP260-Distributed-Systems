# COMP260 Distributed Systems Artefact Proposal
 ### Multiplayer Growth-Based Combat Game
My project will be an online game, drawing inspiration from the game [Agario](https://agar.io/)[1]. My game will be within a 2D map with a top-down perspective. Players will traverse the map as a blob, eating smaller blobs to gain points. The player's radius is directly proportional to their points, so the higher the points, the larger the blob. In the game, a player may consume another player if they are significantly larger in size. Upon consumption, the smaller player is destroyed, and the winning player acquires the points that were previously held by the destroyed player. The game will track each player's highest score, providing a long-term competitive element to the gameplay.

The game state, along with a database holding player data, will be held on a server, where each player will connect from their own, machine.

The game will be developed in Unity. My primary rationale behind this decision is due to Unity's extensive documentation and the supportive community that has been established over the years. Additionally, Unity provides ample resources for networking, allowing me to implement and learn through programming the solution myself, rather than relying solely on pre-built modules, as would be the case in Unreal.

### The Problem

When players want to connect to each other over a network, there are ultimately two ways; LAN (local area network) and WAN (Wide Area Network). 

Latency is the term given when there is any delay in a network. In games, this often manifests as a player giving an input and then waiting e.g. 100ms or 200ms, before that input is accepted as part of the game state. This negatively affects the player experience[2] due to interrupting the flow of the game. Latency occurs on a WAN mainly due to:
- The broad distances that packet data must travel across the network. Even fibre optic cables are 32% slower than the speed of light in a vacuum[6], causing unavoidable latency.
- Protocol confirmation, such as TCP needing a response, means that the delay for a message sent is doubled. 
- The player's machine or the game server's tick rate being slow.


*Netcode* is the name given to the client and server code that connects players to a multiplayer game and attempts to handle the problems caused by latency. I will investigate different methods of netcode solutions to compare their effectiveness at handling latency, thus improving the player experience. 

### Netcode

The standard[4], *old-school* attempt at connecting two players was through a peer-to-peer topology. This progressed onto server-client with an authoritative host. While both are still used today, the standard is server-client with an authoritative dedicated game server, this is the topology I will be using, as it can handle lots of different players connecting [3] while being more economically viable than large-scale cloud options as a student. Different netcode solutions can be implemented on top of these topologies, these are the methods I will compare against one another.

- **Basic Lockstep**. This solution deals with latency by effectively doing nothing. Each player sends their updated game state to the server and the server waits for all messages to arrive; only then does it process the next tick.
<img src="\Documentation\Proposal Images\Basic Lockstap Diagram.png" alt="" width="200"/>
- <img src="" alt="" width="200"/>
- **Deterministic Lockstep**. Rather than sending game states, players send only their inputs, the server holds, updates and returns the game state to all players.
- **Deterministic Lockstep with Input Delay**. Works similar to the above, except the inputs sent, are scheduled for a future tick rather than the current, increasing the server tick rate.
- **Prediction and Rollback**. The server predicts player inputs and uses the prediction if the real input does not arrive. When the actual input arrives, if the prediction is incorrect, the game is *rolled* back to synchronize the state with other players. In ideal conditions, this approach approaches LAN speeds[5].

### Development 
Developed in Unity, each player is represented as a member of a player class with full 2D movement capabilities. The collision between players is determined by the distance between their centres and their current blob radius.
```
distance = sqrt((blob_2.x-blob_1.x)^2 + (blob_2.y-blob_1.y)^2)
if distance < blob_1 + blob_2
{
    # Circles are colliding
}
```
If one blob is significantly larger, it will consume the other
```
if (blob_1.points > blob_2.points+100)
{
	# Blob 1 consumes blob 2
	blob_1.points += blob_2.points;
	blob_2.kill();
}
```


---
##### References 
[1] M. Valadares, _Agar.io_, 28-Apr-2015. [Online]. Available: https://agar.io/. [Accessed: 03-Feb-2023].
[2] S. Vlahovic, M. Suznjevic and L. Skorin-Kapov, "Challenges in Assessing Network Latency Impact on QoE and In-Game Performance in VR First Person Shooter Games," 2019 15th International Conference on Telecommunications (ConTEL), Graz, Austria, 2019, pp. 1-8, doi: 10.1109/ConTEL.2019.8848531.
[3] F. Lu, S. Parkin, and G. Morgan, “Load balancing for massively multiplayer online games,” _Proceedings of 5th ACM SIGCOMM workshop on Network and system support for games - NetGames '06_, 2006.
[4] Y. G. (Meseta), “Netcode Concepts Part 3: Lockstep and rollback,” _Medium_, 22-Sep-2019. [Online]. Available: https://meseta.medium.com/netcode-concepts-part-3-lockstep-and-rollback-f70e9297271. [Accessed: 03-Feb-2023].
[5] H. Teahouse, “GGPO Backroll,” _GitHub_, 03-Oct-2019. [Online]. Available: https://github.com/HouraiTeahouse/Backroll/tree/master/. [Accessed: 03-Feb-2023].
[6] F. Poletti, N. V. Wheeler, M. N. Petrovich, N. Baddela, E. Numkam Fokoua, J. R. Hayes, D. R. Gray, Z. Li, R. Slavík, and D. J. Richardson, “Towards high-capacity fibre-optic communications at the speed of light in vacuum,” _Nature Photonics_, vol. 7, no. 4, pp. 279–284, 2013.