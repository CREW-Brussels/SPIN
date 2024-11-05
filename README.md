# SPIN (SPatial Interface Network)
SPIN is an app that runs on a a Vive Focus 3 (it probably works on other Vive VR hedsets) and acts as a bridge to use [Vive Trackers](https://www.vive.com/us/support/vive-xr/category_howto/installing-tracker-accessories.html) such as Vive Ultimate Trackers on other platforms.

SPIN uses [Open Sound Control (OSC)](https://en.wikipedia.org/wiki/Open_Sound_Control) to stream the trackers position and orientation on the network as well as battery status.

## Demo Video 
[![IMAGE ALT TEXT HERE](https://img.youtube.com/vi/atiAZ8TPYQ0/0.jpg)](https://www.youtube.com/watch?v=atiAZ8TPYQ0 )

## Data format
The data uses Unity's coordinate system (left handed, Y up), each data packet is composed of a position vector, a rotation quaternion and a battery status float as an float[8] array.
`{position.x, position.y, position.z, rotation.w, rotation.x, rotation.y, rotation.z, battery}`

## Installing
Download the latest version [here](https://github.com/CREW-Brussels/SPIN/releases) or build from source.

[Installing APK files on the headset](https://www.vive.com/us/support/focus3/category_howto/installing-apk-on-headset.html).

## How to use SPIN
Spin is intended to be left unattended (the proximity sensor should be blocked or the data rate will be limited to 1Hz) all configuration must be done through a web browser on port 8081 (the IP address is displayed in the headset for convenience).

### Configuration Interface

Once the application is launched connect to the configuration interface at `http://<headset-ip-address>:8081/`.

![image](https://github.com/user-attachments/assets/93ad3850-878c-4ea1-9216-bdd9bbbb9786)

The interface is organized in 4 sections:

#### Host
*Device Name* is the name that will be used as the first part of the OSC address.

*Refresh Rate* is the target frequency for the data packets.

#### Servers
OSC specification defines an application that receives OSC Packets as a "server".

This section allows addition, suppression and modification of "Servers", meaning network devices that will receive the OSC packets.

#### Roles
This section defines the data that will be sent.
Changes can be made at runtime, for example to replace a low battery tracker with a fresh one with minimal interruption.

Each Role corresponds to one OSC address, may target one or more Servers and receives it's data from exactly one tracker.

*name* is not used anywhere and only intended to identify the roles in the configuration interface.

*address* is the last part of the OSC address.

*active* when false the Role will remain in the configuration but will not be sending any data.

*servers* is a list of target servers for this role.

*tracker* is the data source for this role.

#### Trackers
This section allows monitoring of the trackers, it shows witch trackers are online, their tracking status and battery level.

### Example

In this example we attach Tracker1 to a cart and Tracker0 to a door.

Tracker1's position and rotation will be sent to `broadcast` and `Another Server` as /SPIN/cart 50 times per seconds.

Tracker0's position and rotation will be sent to `broadcast` as /SPIN/door 50 times per seconds.

![image](https://github.com/user-attachments/assets/9adb714a-c1ea-43a5-b45a-7d51ac31f6bf)
