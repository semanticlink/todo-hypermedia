import Vue from 'vue';

/**
 * This is a global event bus for communicating between independent components. It should not be used
 * for communication between children back to parent. Vue has a different convention for this.
 *
 * See https://alligator.io/vuejs/global-event-bus/
 *
 * The event bus is a publish-subscribe pattern will be globally available and accessible by es import.
 *
 * Please create constants for all global events so that we can track and understand their purpose.
 *
 *
 * @example
 *
 *  Subscribe (receive the events)
 *  ==============================
 *
 *  // Import the EventBus.
 *  import EventBus, { unauthorised } from './EventBus.js';
 *
 *   Option One: directly handle
 *
 *      // Listen for the event and its payload.
 *      EventBus.$on(unauthorised, args => {
 *         // code here
 *      });
 *
 *   Option Two: handler
 *
 *      const handler = arg => {}
 *      EventBus.$on(unauthorised, handler)
 *
 *   Note: options is useful if you need to deregister handlers (see below)
 *
 *  Publish (send the events)
 *  ========================
 *
 *      // Import the EventBus.
 *      import EventBus, { unauthorised } from './EventBus.js';
 *
 *
 *      //some function and inside
 *      function(){
 *          // Send the event and payload
 *      EventBus.$emit(unauthorised, 'object');
 *
 *  Removing events
 *  ===============
 *
 *  Option One: de-resigister all listeners to an event
 *
 *      EventBus.$off(unauthorised)
 *
 *  Option Two: de-register specific listeners to an event
 *
 *      EventBus.$off(unauthorised, handler)
 *
 *  Options Three: de-regisiter all listeners to all events
 *
 *      EventBus.$off()
 *
 */
const EventBus = new Vue();

export default EventBus;
