import {log} from "semantic-link/lib/logger";

/**
 * Event bus that can be implemented per client-side framework. EventBus allows publish-subscribe-style
 * communication between components without requiring the components to explicitly register with
 * one another (and thus be aware of each other).
 *
 * This interface is modelled in Vue components.
 *
 *
 * @example
 *
 * This is a global event bus for communicating between independent components. It should not be used
 * for communication between children back to parent. Vue has a different convention for this.
 *
 * See https://alligator.io/vuejs/global-event-bus/
 *
 * The event bus is a publish-subscribe pattern will be globally available and accessible by es import.
 *
 * Please create constants for all global events so that we can track and understand their purpose.

 *  Create Bus
 *  ==========
 *
 * @example
 *
 *   import Vue from 'vue';
 *   const EventBus = new Vue();
 *   export default EventBus;
 *
 *
 *  Subscribe (receive the events)
 *  ==============================
 *
 *  // Import the EventBus.
 *  import EventBus from './EventBus.js';
 *  import { unauthorised } from './EventMessages.js';
 *
 *  @example Option One: directly handle
 *
 *
 *      // Listen for the event and its payload.
 *      EventBus.$on(unauthorised, args => {
 *         // code here
 *      });
 *
 *  @example Option Two: handler
 *
 *      const handler = arg => {}
 *      EventBus.$on(unauthorised, handler)
 *
 *   Note: options is useful if you need to deregister handlers (see below)
 *
 *  Publish (send the events)
 *  ========================
 *
 *  @example
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
 *  @example Option One: de-resigister all listeners to an event
 *
 *      EventBus.$off(unauthorised)
 *
 *  @example Option Two: de-register specific listeners to an event
 *
 *      EventBus.$off(unauthorised, handler)
 *
 *  @example sOptions Three: de-regisiter all listeners to all events
 *
 *      EventBus.$off()
 *
 */


export default interface EventBus {

    $on(event: string, callback: Function): void

    $emit(event: string, callback: Function): void

    $off(event: string, callback: Function): void
}

/**
 * Currently the interface of this is based on Vue
 * @type {EventBus}
 */
export let eventBus: EventBus;

/**
 * Inject an {@link EventBus} to be used
 * @param {EventBus} instance
 * @return {*}
 */
export const setEventBus = (instance: EventBus) => {
    log.debug('[EventBus] Set event bus');
    eventBus = instance;
};


