import { expect } from 'chai';
import { stateFlagEnum } from './stateFlagEnum';
import State from './State';

describe('StateFactory', () => {

    it('should be create an object with state', () => {
        const obj = State.make();
        expect(obj).to.not.be.null;
        expect(State.get(obj).getStatus()).to.equal(stateFlagEnum.unknown);
    });

    it('should return undefined when tryGet on object with no state', () => {
        expect(State.tryGet({})).to.be.undefined;
    });

    it('should return default value when tryGet on object with no state', () => {
        expect(State.tryGet({}, 'a')).to.equal('a');
    });

    it('should be able to remove state', () => {
        const obj = State.make();
        expect(obj).to.not.be.null;
        expect(State.delete(obj).getStatus).to.be.undefined;
    });
});