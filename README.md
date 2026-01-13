# Debugger for Jasm-based state machines

Provides visual debugging capabilities for state machines implemented using the Jasm framework.
This debugger allows developers to inspect the current state of all connected state machines in real-time.  

To use this debugger the state machine must be registered at the debug-adapter. The adapter will connect to the debugger and relay state information.
For more information on how to set up the debug-adapter, please refer to the debug-adapter documentation for the specific state machine.

Currently only Jasmsharp is supported.