<div align="center">

# 🖩 Windows Calculator

![Windows Calculator](https://img.shields.io/badge/Platform-Windows-blue)
![C#](https://img.shields.io/badge/Language-C%23-green)
![WPF](https://img.shields.io/badge/Framework-WPF-red)

### ⚡ A modern, feature-rich calculator application for Windows built with WPF (.NET). ⚡

</div>

## ✨ Features

### 🔢 Multiple Calculation Modes

-   **Standard Calculator** – Perform basic arithmetic operations with a clean, user-friendly interface.
-   **Programmer Calculator** – Work with different number bases, including Hexadecimal, Decimal, Octal, and Binary.

### 🏆 Standard Calculator Features

-   Basic arithmetic operations (addition, subtraction, multiplication, division).
-   Special functions: square root (√), square (x²), reciprocal (¹/x).
-   Percentage calculations (%).
-   Value negation (±).
-   Decimal point support.
-   Memory functions:
    -   🗑️ **MC (Memory Clear)** – Clears all stored values.
    -   📋 **MR (Memory Recall)** – Displays the last stored value.
    -   ➕ **M+ (Memory Add)** – Adds the current value to memory.
    -   ➖ **M- (Memory Subtract)** – Subtracts the current value from memory.
    -   💾 **MS (Memory Store)** – Stores the current value in memory.

### 🖥️ Programmer Calculator Features

-   Support for multiple number bases:
    -   **Hexadecimal (HEX)** – 0-9, A-F.
    -   **Decimal (DEC)** – 0-9.
    -   **Octal (OCT)** – 0-7.
    -   **Binary (BIN)** – 0-1.
-   Real-time base conversion.
-   Arithmetic operations in any number base.

### 📋 Clipboard Support

-   **Cut** – Remove the current displayed value.
-   **Copy** – Copy the current displayed value.
-   **Paste** – Paste a value from the clipboard.

### 🎨 User Interface

-   Modern, Windows 11-style design.
-   Custom window frame with minimalist controls.
-   Hamburger menu for easy navigation.
-   Settings persistence between sessions.
-   Responsive keyboard input.

### ⚙️ Settings & Persistence 💾

-   **Configurable Options:**

    -   Digit grouping toggle **(e.g., 1,234,567 vs 1234567)**
    -   Calculator mode selection **(Standard/Programmer)**
    -   Number base preference **(HEX/DEC/OCT/BIN)**

-   **Automatic Persistence:**
    -   All settings are saved between application sessions
    -   Settings are stored in a JSON file in the user's AppData folder
    -   Changes are saved immediately when preferences are modified
    -   Settings are automatically loaded on application startup
