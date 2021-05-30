# Multi-agent emergence with Unity

## Objective
Objective of the project is to reproduce results from OpenAI paper ["Emergent Tool Use From Multi-Agent Autocurricula"](https://arxiv.org/abs/1909.07528) ([Blog page](https://openai.com/blog/emergent-tool-use/)) by using free [Unity](https://unity.com/) engine instead of [MuJoCo](http://www.mujoco.org/). In the paper researches observed how agents learned to use different objects while playing hide-and-seek game in order to beat the opposite team.

## Hide-and-seek game
There two teams of agents: hiders and seekers. The task of hiders is to avoid seekers line of sight and the task of seekers is to keep hiders in sight. Agents can move, turn around, grab and lock special objects in the environment. Seekers get positive reward and hiders get the same negative reward if any of the hiders are seen otherwise rewards are mirrored. Agents also penalized if they escape from playing area, it is possible because of physics engine imperfection, and it is a winning strategy for hiders (they get it quickly enough) because seekers can not deal much with it while this is not very interesting case, same problem was met in the paper. There is also preparation phase when seekers are disabled and rewards are not given.

For more details see [paper](https://arxiv.org/abs/1909.07528) or [OpenAI blog page](https://openai.com/blog/emergent-tool-use/).
## Agents
Agents' actions and observations are made as in the paper
### Observations
*x* denotes two dimensional position vector and *v* denotes two dimensional velocity vector below

| Observation | Details |
|-|-|
| *x* and *v* of self |  |
| Lidar | Range sensors arrayed evenly around the agents |
| Line of sight | Cone in front of the agent. Agent get information about other object only if it gets inside the cone |
| *x*, *v*, *label* of another object | *label* indicates whether the object is hider, seeker, box or ramp |

### Actions
| Action | Details |
|-|-|
| Apply force to self | Applying force to the agent makes it move |
| Add torque to self | Adding torque makes agent turn around |
| Grab object | Agent can grab an object and move it around |
| Drop object | Agent can drop the object which was grabbed |
| Lock object | Agent can lock an object so object can't be moved or grabbed by anyone else |
| Unlock object | Agent can cancel object lock if it was made by the agent from the same team |

### Training
Agents are improved with reinforcement learning. The required algorithms are already implemented in the [Unity ML-Agents Toolkit](https://github.com/Unity-Technologies/ml-agents) which allows to train agents in environment created with Unity.

### Controls
To manipulate agents manually use the following keys

| Key | Action |
|-|-|
| `W,A,S,D` | Add force along one of directions |
| `Q,E` | Add torque to rotate counter-clockwise or clockwise |
| `1` | Drop object |
| `2` | Grab object |
| `3` | Unlock object |
| `4` | Lock object |

## Environments
At this moment only one environment is implemented.

### Simple Environment
In simple environment there is a room with two holes placed randomly, 2 agents in each team, 2 boxes and ramp. Hiders and boxes spawn randomly inside the room, seekers and the ramp spawn randomly outside the room.

![Simple environment image][https://github.com/DT6A/multi-agent-emergence-unity/img/env3.PNG]

Video demonstration:

[![Video demosntration](https://img.youtube.com/vi/Bk1vIVzacZs/0.jpg)](https://www.youtube.com/watch?v=Bk1vIVzacZs)
## Installation

## Experiments
