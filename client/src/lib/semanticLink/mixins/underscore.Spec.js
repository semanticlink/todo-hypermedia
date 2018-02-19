import _ from './underscore';
import { expect } from 'chai';

describe('All underscore mixins', () => {
    it('should expose _ object', () => {
        expect(_).to.not.be.null;
    });

    it('should use from one of the files', () => {
        expect(_({name: 'John'}).compactObject()).to.deep.equal({name: 'John'});
    });
});