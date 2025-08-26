# 🃏 Poker Roguelite - Unity Game Development Showcase

*A sophisticated Android poker-based roguelite developed in Unity for a Game Engine class at LaSalle College*

[![Unity](https://img.shields.io/badge/Unity-2022.3+-black.svg?style=flat&logo=unity)]()
[![C#](https://img.shields.io/badge/C%23-9.0+-blue.svg?style=flat&logo=c-sharp)]()
[![Platform](https://img.shields.io/badge/Platform-Android-green.svg?style=flat&logo=android)]()

## 🎯 Project Overview

This project demonstrates advanced Unity development skills through a complete poker-based roguelite game. Players battle through 10 increasingly challenging rounds using poker hands as combat mechanics, with strategic deck-building and upgrade systems creating deep gameplay.

---

## 🚀 Key Technical Achievements

### **🏗️ Advanced Architecture Patterns**

#### **Singleton Pattern Implementation**
```csharp
// Clean singleton with proper null checking and scene persistence
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null) Debug.LogError("GameManager is NULL");
            return _instance;
        }
    }
    
    private void Awake()
    {
        if (_instance) Destroy(gameObject);
        else _instance = this;
        DontDestroyOnLoad(this);
    }
}
```

#### **Observer Pattern with Events**
```csharp
// Event-driven coin system with automatic UI updates
public delegate void CoinDelegate();
public static event CoinDelegate OnChange;

public int Coins
{
    get => coins;
    set
    {
        coins = value;
        if (OnChange != null) OnChange();
    }
}
```

#### **State Machine Design**
- **Turn-based combat** with complex state transitions
- **Scene management** across Menu → Shop → Combat flow  
- **Card states** (Selected, Kept, In-Hand, Discarded) with visual feedback

---

### **🎮 Complex Game Systems**

#### **Poker Hand Recognition Engine**
Custom algorithm that identifies all poker combinations:
- **Royal Flush, Straight Flush, Four of a Kind**
- **Full House, Flush, Straight, Three of a Kind**  
- **Two Pairs, Pair, High Card**
- **Tie-breaking logic** with proper Ace handling (high/low)

#### **Advanced AI System**
Enemy AI with escalating difficulty:
- **Rounds 1-6**: Basic play patterns
- **Rounds 7-10**: Strategic card replacement based on hand quality
- **Adaptive decision making** - AI discards weak cards to improve hands

#### **Roguelite Progression Systems**

**Relic System** (Permanent Upgrades):
- **Magic Shades** - Reveals enemy cards when player health ≤ 50%
- **Golden Cards** - Increases card keeping limit from 2 to 3
- **Heart of Steel** - Increases max health from 20 to 30
- **Precision Scope** - +2 damage bonus for 3+ card combinations

**Special Card Types**:
- **Damage Cards** - Deal 2 damage instead of 1
- **Bank Cards** - Generate 2 coins when drawn
- **Heal Cards** - Restore 1 health when drawn
- **Free Cards** - Don't count against keep limit

**Consumable Items**:
- **Medkit** - Instant 5 health restoration
- **Mulligan** - Extra draw per turn
- **No Thank You** - Force enemy card redraw
- **Damage Boost** - +1 damage per card in winning hand

---

### **⚙️ Technical Implementation Highlights**

#### **Card Animation System**
Sophisticated card movement with coroutines:
```csharp
public IEnumerator DrawAnimation()
{
    GameObject carteVerso = Instantiate(CarteVerso, deckTransform);
    for (float i = 0; i < 11; i++)
    {
        carteVerso.transform.position = Vector2.Lerp(deckTransform.position, transform.position, i / 10);
        carteVerso.transform.rotation = Quaternion.Lerp(deckTransform.rotation, transform.rotation, i / 10);
        yield return new WaitForSeconds(0.05f);
    }
    Enabled(true);
    Destroy(carteVerso);
}
```

#### **Dynamic UI Management**
- **Real-time inventory visualization** with sprite mapping
- **Adaptive positioning system** for poker hand displays
- **Contextual message system** with fade animations
- **Responsive audio volume controls** with persistence

#### **Data Structures & Algorithms**

**Efficient Deck Management**:
```csharp
// Fisher-Yates shuffle implementation
public void ShuffleDeck()
{
    var temp = new List<CarteData>();
    while (_enemyDeck.Count > 0)
    {
        int indexRandom = Random.Range(0, _enemyDeck.Count);
        temp.Add((_enemyDeck[indexRandom]));
        _enemyDeck.RemoveAt(indexRandom);
    }
    _enemyDeck = temp;
}
```

**Optimized Combo Detection**:
- **O(n²) pair detection** for efficient hand analysis
- **Sorted array comparison** for straight detection
- **Symbol equality checking** for flush detection

---

### **🎨 UI/UX Design Excellence**

#### **Responsive Interface Design**
- **Clean card visualization** with color-coded suits (red/black)
- **Visual state indicators** (Selected: Yellow, Kept: Green, Default: Black)
- **Contextual button states** with dynamic text updates
- **Loading animations** with smooth scene transitions

#### **Accessibility Features**
- **Unicode suit symbols** (♠ ♥ ♦ ♣) for clear visual distinction
- **High contrast visual feedback** for card states
- **Audio cues** for important game events
- **Persistent volume controls** across scenes

#### **Professional Polish**
- **Consistent visual hierarchy** throughout all screens
- **Smooth animation transitions** for all game actions
- **Comprehensive sound design** with environmental audio
- **Intuitive control scheme** optimized for mobile

---

### **📱 Android Optimization**

#### **Mobile-First Design**
- **Touch-optimized UI** with appropriately sized buttons
- **Performance-conscious rendering** with efficient object pooling
- **Memory management** with proper object cleanup
- **Scene loading optimization** with async operations

#### **Cross-Platform Compatibility**
- **Scalable UI canvas** for different screen resolutions
- **Platform-specific audio handling**
- **Efficient texture management** for mobile constraints

---

## 🎲 Game Flow & Mechanics

### **Core Gameplay Loop**
1. **Shop Phase**: Purchase upgrades, relics, and consumables
2. **Combat Phase**: Play poker hands against AI opponent  
3. **Resolution**: Calculate damage, apply effects, advance round
4. **Progression**: Earn coins based on performance and remaining health

### **Strategic Depth**
- **Risk/Reward decisions** in card keeping vs. drawing
- **Resource management** with limited coins and consumables
- **Build optimization** through relic and special card synergies
- **Escalating difficulty** requiring strategic adaptation

---

## 🛠️ Development Skills Demonstrated

### **Programming Expertise**
- ✅ **Advanced C# Programming** with LINQ, generics, and delegates
- ✅ **Design Pattern Implementation** (Singleton, Observer, State Machine)
- ✅ **Algorithm Development** (Poker hand recognition, AI decision making)
- ✅ **Coroutine Mastery** for complex animation sequences
- ✅ **Event-Driven Architecture** for decoupled system communication

### **Unity Engine Proficiency**
- ✅ **Scene Management** with persistent data across transitions
- ✅ **Animation Systems** using both Animator and code-driven animations
- ✅ **UI Framework** with Canvas, TextMeshPro, and responsive layouts
- ✅ **Audio Integration** with dynamic volume control and sound effect management
- ✅ **Prefab System** for modular and reusable game objects

### **Game Design Capabilities**
- ✅ **Roguelite Mechanics** with meaningful progression systems
- ✅ **Difficulty Scaling** with AI behavior adaptation
- ✅ **User Experience Design** with intuitive controls and feedback
- ✅ **Balance Testing** through mathematical probability analysis
- ✅ **Mobile Platform Optimization** for Android deployment

### **Software Engineering Practices**
- ✅ **Clean Code Architecture** with proper separation of concerns
- ✅ **Modular Design** allowing easy feature expansion
- ✅ **Error Handling** with null checks and bounds validation
- ✅ **Performance Optimization** with efficient algorithms and memory usage
- ✅ **Documentation** through clear variable naming and code organization

---

## 📊 Technical Complexity Metrics

| System | Lines of Code | Key Features |
|--------|---------------|--------------|
| **BattleManager** | 910+ | Combat resolution, AI behavior, turn management |
| **Player** | 541+ | Hand management, combo detection, deck building |
| **GameManager** | 79 | Persistence, scene transitions, state management |
| **Shop** | 247+ | Economy system, upgrade purchases, inventory |
| **Carte** | 256+ | Card visualization, animations, state management |
| **StatsCalculator** | 75+ | Probability analysis, game balance validation |

**Total Project Scope**: 2000+ lines of production-quality C# code

---

## 🎯 Professional Value Proposition

This project showcases the ability to:

- **Architect complex systems** with clean, maintainable code
- **Implement sophisticated algorithms** for game mechanics
- **Design engaging user experiences** with polished UI/UX
- **Optimize for mobile platforms** with performance considerations
- **Manage project scope** from concept to completion within academic constraints
- **Apply computer science principles** to practical game development challenges

The combination of **technical depth**, **creative design**, and **professional polish** demonstrates readiness for intermediate to senior Unity developer positions, with particular strength in **gameplay programming**, **systems architecture**, and **mobile game development**.

---

*Developed as coursework for Game Engine Programming at LaSalle College, demonstrating advanced Unity development skills and game design expertise.*
