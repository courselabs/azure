# Lab Suggestions

If you needed to add a new supplier to this quote engine, you'd need to write a new activity function with the call to the supplier's API. Then the orchestrator would need to be changed to call the new activity and wait for it along with the rest. Deploying the change would mean updating the existing function - if there were any issues with the new activity then it would break the existing workflow.

With a pub-sub pattern you'd add the new supplier code in a function triggered from a Service Bus queue or topic. That gets deployed on its own without needing any changes to existing code and any issues are isolated to the new component.

If you needed to put a time limit on the whole workflow, you can implement timeouts in the individual function, or a timeout inside a durable function. When the timeout fires, the orchestrator selects the best from the quotes it has received and doesn't wait for any remaining suppliers.