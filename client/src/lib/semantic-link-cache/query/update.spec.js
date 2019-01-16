import {expect} from 'chai';
import {update} from './update';
import sinon from 'sinon';
import * as cache from 'src/lib/semantic-link-cache/cache/cache';

global.Element = () => {
};

const stub = methodName => sinon.stub(cache, methodName)
    .returns(Promise.resolve());


describe('Update', () => {

    let resourceFactory;

    const singleton = {links: []};

    it(cache.updateResource.name, () => {
        resourceFactory = stub(cache.updateResource.name);
        return update(singleton, singleton);
    });

    afterEach((() => {
        expect(resourceFactory.called).to.be.true;
        resourceFactory.restore();
    }));

});
