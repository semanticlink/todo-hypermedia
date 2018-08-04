import EventBus, { authRequired } from './EventBus';
import { expect } from 'chai';

describe('Event Bus', () => {

    it('should be able to subscribe to event', () => {

        return new Promise(pass => {
            EventBus.$on(authRequired, () => {
                pass();
            });

            EventBus.$emit(authRequired);

        });

    });

    it('should be able to subscribe to event with args', () => {

        return new Promise(pass => {
            EventBus.$on(authRequired, val => {
                expect(val).to.equal('test');
                pass();
            });

            EventBus.$emit(authRequired, 'test');
        });

    });

});
