import EventBus, { loginRequired } from './EventBus';
import { expect } from 'chai';

describe('Event Bus', () => {

    it('should be able to subscribe to event', () => {

        return new Promise(pass => {
            EventBus.$on(loginRequired, () => {
                pass();
            });

            EventBus.$emit(loginRequired);

        });

    });

    it('should be able to subscribe to event with args', () => {

        return new Promise(pass => {
            EventBus.$on(loginRequired, val => {
                expect(val).to.equal('test');
                pass();
            });

            EventBus.$emit(loginRequired, 'test');
        });

    });

});
