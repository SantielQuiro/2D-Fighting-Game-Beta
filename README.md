# 🎮 7Hit Stick Fight

## 🕹️ Play the Game: 👉 [Play on Itch.io]((https://santielquiro.itch.io/7hit-stick-fight-beta))

## 📌 Description
7Hit Stick Fight is a 2D Action Game focused on timing-based combat, 
featuring stamina management and a parry-centered combat system.
Player must time their Attacks and Parries carefuly to defeat the enemy, 
while managing Stamina

---

## ⚙️ Features

- Player movement system
- Parry-based combat mechanics
- Enemy AI using a finite state machine
- Stamina system
---
## 🧠 Systems implemented

### Enemy AI (Finite State Machine)
The enemy behaviour is controlled using a state machine with the following states:
- Idle
- Chase
- Attack
- Strong Attack
- Block
- Dash
- Retire
- Hurt
- Stunned / Critical State

The AI reacts dynamically to player distance and combat interactions.

### 💥Combat

- Parry-focused combat design
- Basic attack system
- Hit detection system
- Scripted events for combat interactions

---

## 🛠️ Tech
- Unity
- C#
---
## 👨‍💻 My role
Solo developer:
- Full gameplay programming
- AI design and implementation
- Combat system development
---
## ⚡ Code Highlights
- Modular AI State system
- Separation Between Context (environment information), Functions (AI State Logic), and Brain (AI making decisions)
- Health and Stamina are generic and reusable scripts
- Scripted Events 
