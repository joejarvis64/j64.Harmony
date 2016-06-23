/**
 *  j64 - Channel Switch
 *
 *  Copyright 2016 joejarvis64@gmail.com
 *
 *  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except
 *  in compliance with the License. You may obtain a copy of the License at:
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
 *  on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License
 *  for the specific language governing permissions and limitations under the License.
 *
 */
metadata {
	definition (name: "j64 Custom Command Switch", namespace: "j64", author: "joejarvis64@gmail.com") {
		capability "Switch"
		capability "Refresh"
        }

	simulator {
	}

    tiles {
        standardTile("switch", "device.switch", width: 3, height: 3, canChangeIcon: true) {
            state "off", label: 'Run Command', action: "switch.on",  icon: "st.Electronics.electronics3", backgroundColor: "#aae0f0"
            state "on", label:  'Run Command', action: "switch.off", icon: "st.Electronics.electronics3",  backgroundColor: "#aae0f0"
        }

        standardTile("switchMain", "device.switch", width: 3, height: 3, canChangeIcon: true) {
            state "off", label: 'Run', action: "switch.on",  icon: "st.Electronics.electronics3-icn", backgroundColor: "#aae0f0"
            state "on", label:  'Run', action: "switch.off", icon: "st.Electronics.electronics3-icn",  backgroundColor: "#aae0f0"
        }

        main "switchMain"
            details(["switch"])
      }
	}

// parse events into attributes
def parse(String description) {
	log.debug "Parsing '${description}'"
}

// handle commands
def on() {
	changeChannel()
}

def off() {
	changeChannel()
}

def changeChannel() {
	def command = device.deviceNetworkId.replaceAll("j64HarmonyCommand-","")
	log.debug "Custom command ${command}"
    command = URLEncoder.encode(command)
	parent.RunCommand(command)
}
