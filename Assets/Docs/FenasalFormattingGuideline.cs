/*

// Unity C# Formatting Guide

// 1. K&R style puts opening brace on the previous line.
void DisplayMouseCursor(bool showMouse){
     if (!showMouse) {
          Cursor.lockState = CursorLockMode.Locked;
          Cursor.visible = false;
     }

}

// 2. Curly braces are optional for single-line nested blocks. 
// Use them for better readability, but they can be omitted for simple, one-line statements.
for (int i = 0; i < 10; i++)
    for (int j = 0; j < 10; j++)
        ExampleAction();

// 3. Curly braces are optional for single-line statements. 
// You can omit them for short, simple statements, but they can be used for clarity.
if (isActive)
    DoSomething();

// 4. Leave Spaces Before and After Operators - Always leave spaces around operators.
int sum = a + b;
bool isValid = x == y;

// 5. No Spaces Before and After Condition Parentheses - Do not leave spaces around condition parentheses.
if (value > 10)
    DoSomething();

// 6. Use a Single Space Between Parameters in Function Calls - Always use a single space between parameters in function calls.
CollectItem(myObject, 0, 1);

// 7. Break Long Lines Appropriately - Break long lines into multiple lines for readability.
string message = "This is a very long string that should " +
                 "be split across multiple lines for readability.";

// 8. Place Using Directives at the Top of the File - Place `using` directives at the top of the file in alphabetical order.
using System;
using System.Collections;
using UnityEngine;

// 9. Fields Should Be Private - All fields should be private and can be named using camelCase.
private int playerHealth;

// 10. Constants Should Be Named in All Uppercase with Underscores - Constants should be named in all uppercase with underscores.
private const int MAX_LIVES = 3;

// 11. Methods Should Be Named Using PascalCase - Methods should be named using PascalCase.
void StartGame(){
    Debug.Log("Game Started");
	}

// 12. Variables Should Be Named Using camelCase - Variables should be named using camelCase.
int playerScore = 0;

// 13. Each Class Should Be in a Separate File, and Class Names Should Use PascalCase.
public class PlayerController : MonoBehaviour{
    // Player control code goes here.
}

// 14. Comments Should Be Concise and Descriptive.
void UpdateSpeed(){
    // This function updates the player's speed.
    speed = baseSpeed * acceleration;
}

// 15. Use Explicit Access Modifiers for Clarity.
public int health; // Public field
private float speed; // Private field

// 16. Avoid Magic Numbers - Do not use magic numbers in the code. Use constants instead.
private const int MAX_PLAYER_HEALTH = 100;

// 17. Prefer `foreach` Over `for` When Iterating Over Collections.
foreach (var enemy in enemies){
    enemy.TakeDamage(10);
}

*/