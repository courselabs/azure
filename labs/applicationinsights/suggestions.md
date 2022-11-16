# Lab Suggestions

All the data from Application Insights gets stored in Log Analytics, as long as you are using the current architecture. Using separate App Insights apps gives you a tailored experience - in the UI you are only working with one app and you can see the metrics and relevant events without filtering. If every App Insights instance uses its own Log Analytics Workspace, that makes it more difficult (and slower) to query because you have to join across two separate Workspaces.

The opposite approach is to have one App Insights and one Log Analytics Workspace for all your components. That lets you track dependencies and see the full user workflows, but you'll need to filter out the UI when you want to look at a specific component's performance.

The in-between approach is multiple App Insights writing to a central Log Analytics Workspace. That lets you have a focused experience in the App Insights UI but still get a whole-application view in Log Analytics.