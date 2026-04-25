# Unity2D-Slot-Machine

## 🎮 Game Overview
A fully playable, feature-rich 2D Slot Machine game built with Unity. The game features a classic casino aesthetic with modern mechanics, including multi-line winning logic and dynamic bet-based payouts.

### 🕹️ How to Play
1. **Select a Bet**: Click on the **Bet $10**, **Bet $50**, or **Bet $100** buttons to choose your wager. The selected bet will highlight in yellow.
2. **Spin**: Pull the **Lever** on the right side of the machine.
3. **Winning**: Match three symbols across any of the three horizontal lines (Top, Middle, Bottom as there are three lines on the 3 reels).
4. **Collect**: If you win, a celebratory popup appears with a jackpot message. Click **COLLECT** to claim your coins.
5. **Exit**: Click the **Exit** button to close the game (WebGL builds will stop the application).

---

## 📐 Technical Features & Mechanics
- **Multi-Line Win System**: Unlike basic slots, this game checks the Top, Middle, and Bottom lines simultaneously, allowing for multiple wins in a single spin.
- **Tiered Payout Logic**: Winning payouts scale dynamically based on the selected bet amount:
    - $10 Bet -> $50 Win (Per Line)
    - $50 Bet -> $200 Win (Per Line)
    - $100 Bet -> $500 Win (Per Line)
- **Procedural Reel Logic**: The reels use a custom wrapping system that handles infinite spinning, smooth deceleration, and precise snapping to RNG-generated results.
- **Dynamic Lever Animation**: The lever pull is realistically simulated with scripted scale reduction and vertical offset, providing tactile visual feedback.
- **Profit Splash Effects**: Winning amounts float up from the machine with a fade-out animation for immediate gratification.
- **Juiced Animations(like in casino)**:
    - **Reel Stop Bounce**: Reels now feature a realistic "overshoot" bounce when stopping, providing a tactile, high-quality slot machine feel.
    - **Winning Line Pulsing**: Symbols forming a winning combination pulse with a "glow" scale effect to clearly highlight the win through pulsating.
    - **Win Popup Entrance**: The Jackpot window scales up with a satisfying bounce animation using the built-in Animator system.
- **Fully Integrated Audio**: Custom-generated sound effects for spinning (loop), winning fanfare, and a deep descending losing sound effect.

---

## 📁 Project Structure
- `Assets/Scripts/`: Core gameplay logic (`SlotMachineController.cs`, `Reel.cs`).
- `Assets/Prefabs/`: Reusable components like the `Symbol.prefab`.
- `Assets/UI/`: All sprite assets for the machine, symbols, and popups.
- `Assets/Sounds/`: Custom SFX assets (`spin_loop.wav`, `win_fanfare.wav`, `lose_effect.wav`).
- `Assets/Animations/`: Animator Controllers and Animation Clips for UI and Symbols.

---

## 🛠️ Build Instructions (WebGL) 
1. Open the project in Unity 6000.4.3f1+.
2. Ensure the scene `Assets/Scenes/SampleScene.unity` is open.
3. Go to **File > Build Settings**.
4. Select **WebGL** as the platform.
5. Click **Build and Run**.

NOTE: Web Build in already made in this repository which should be run via python server
---

## 🧠 Thought Process & Approach
My approach centered on creating a clean, modular, and scalable slot machine system.
- **Modularity**: The `Reel` class is independent, allowing for any number of reels or symbols without changing the core controller.
- **State Management**: Using coroutines for the spin-to-stop sequence ensures smooth timing and sequential reel stops (Left -> Middle -> Right), enhancing the "near-miss" excitement.
- **Visual Feedback**: Priority was given to "Juice" (lever movement, splashing text, button highlights, audio) to make the game feel interactive and rewarding.
- **RNG Fairness**: Outcomes are determined at the moment of the pull, with the reels procedurally animating to show the pre-determined result.

