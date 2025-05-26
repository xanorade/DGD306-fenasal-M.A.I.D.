using System.Collections.Generic;
using UnityEngine;

namespace DGD306.Character
{
    public enum InputType
    {
        Neutral,
        Up,
        Down,
        Left,
        Right,
        Punch,
        Kick,
        Special,
        Jump,
        Dash,
        Block
    }

    public class InputCommand
    {
        public InputType PrimaryType { get; private set; }
        public InputType DirectionType { get; private set; }
        public float Timestamp { get; private set; }

        public InputCommand(InputType primaryType, InputType directionType = InputType.Neutral)
        {
            PrimaryType = primaryType;
            DirectionType = directionType;
            Timestamp = Time.time;
        }
    }

    public class InputBuffer
    {
        private const int MAX_BUFFER_SIZE = 20;
        private const float BUFFER_WINDOW = 0.5f; // Half-second buffer window
        
        private Queue<InputCommand> commandBuffer = new Queue<InputCommand>();
        
        // Add an input to the buffer
        public void AddInput(InputCommand command)
        {
            commandBuffer.Enqueue(command);
            
            // Make sure buffer doesn't exceed max size
            if (commandBuffer.Count > MAX_BUFFER_SIZE)
                commandBuffer.Dequeue();
        }
        
        // Process the buffer - clean up old commands
        public void ProcessBuffer()
        {
            float currentTime = Time.time;
            
            // Remove expired commands
            while (commandBuffer.Count > 0 && currentTime - commandBuffer.Peek().Timestamp > BUFFER_WINDOW)
            {
                commandBuffer.Dequeue();
            }
        }
        
        // Check if a sequence of inputs has been performed recently
        public bool CheckSequence(InputType[] sequence)
        {
            if (sequence.Length > commandBuffer.Count)
                return false;
                
            // Convert queue to array for easier processing
            InputCommand[] commands = commandBuffer.ToArray();
            
            // Check for the sequence in the buffer
            for (int i = 0; i <= commands.Length - sequence.Length; i++)
            {
                bool match = true;
                
                for (int j = 0; j < sequence.Length; j++)
                {
                    if (commands[i + j].PrimaryType != sequence[j] && 
                        commands[i + j].DirectionType != sequence[j])
                    {
                        match = false;
                        break;
                    }
                }
                
                if (match)
                    return true;
            }
            
            return false;
        }
        
        // Clear the buffer
        public void ClearBuffer()
        {
            commandBuffer.Clear();
        }
    }
} 