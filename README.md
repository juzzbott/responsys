# Responsys

Responsys is a system to help aid crews turning out to requests for assisstance. The system polls the ResponseHub website periodically for a given unit. If new messages are detected, then the details of the job are automatically printed. 

**Note:** *Responsys is not designed to be an alerting platform. Units and members must still using their primary alerting system for responding to requests for assistance.*

# Build and test
The solution contains the following components:

* __Responsys.UI__ - Windows application project
* __Model, Common, Logging__ - Nuget packages from the ResponseHub web application to re-use components.

## Build results
[![Build status](https://juzzbott.visualstudio.com/ResponseHub/_apis/build/status/Responsys%20Application)]