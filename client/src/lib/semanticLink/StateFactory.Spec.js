import { expect } from 'chai';
import { stateFlagEnum } from './stateFlagEnum';
import StateFactory from './StateFactory';

describe('StateFactory', () => {

    it('should be create an object with state', () => {
        const obj = StateFactory.make();
        expect(obj).to.not.be.null;
        expect(StateFactory.get(obj).getStatus()).to.equal(stateFlagEnum.unknown);
    });

    it('should return undefined when tryGet on object with no state', () => {
        expect(StateFactory.tryGet({})).to.be.undefined;
    });

    it('should return default value when tryGet on object with no state', () => {
        expect(StateFactory.tryGet({}, 'a')).to.equal('a');
    });

    it('should be able to remove state', () => {
        const obj = StateFactory.make();
        expect(obj).to.not.be.null;
        expect(StateFactory.delete(obj).getStatus).to.be.undefined;
    });
});