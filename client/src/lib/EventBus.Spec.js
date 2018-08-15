import EventBus from './EventBus';
import {expect} from 'chai';
import {eventBus, setEventBus} from 'semantic-link-utils/EventBus';

const authRequired = 'event:message';


describe('Vue', () => {

    it('should be able to subscribe to event', () => {

        return new Promise(pass => {
            EventBus.$on(authRequired, () => {
                EventBus.$off(authRequired);
                pass();
            });

            EventBus.$emit(authRequired);

        });

    });

    it('should be able to subscribe to event with args', () => {

        return new Promise(pass => {
            EventBus.$on(authRequired, val => {
                EventBus.$off(authRequired);
                expect(val).to.equal('test');
                pass();
            });

            EventBus.$emit(authRequired, 'test');
        });

    });

});

describe('Event Bus implementation', () => {

    setEventBus(EventBus);

    it('should be able to subscribe to event', () => {

        return new Promise(pass => {
            eventBus.$on(authRequired, () => {
                EventBus.$off(authRequired);
                pass();
            });

            eventBus.$emit(authRequired);

        });

    });

    it('should be able to subscribe to event with args', () => {

        return new Promise(pass => {
            eventBus.$on(authRequired, val => {
                EventBus.$off(authRequired);
                expect(val).to.equal('test');
                pass();
            });

            eventBus.$emit(authRequired, 'test');
        });

    });

});

