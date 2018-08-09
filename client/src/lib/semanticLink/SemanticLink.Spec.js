import {log} from 'logger';
import sinon from 'sinon';
import { SemanticLink } from './SemanticLink';
import { expect } from 'chai';

// NOTE: we could use moxios to stub out axios
const GET = (url, timeout = 0) => {
    return {method: 'GET', url: url, timeout: timeout, data: null};
};

describe('SemanticLink', () => {

    let $http;
    /**
     * @type {SemanticLink}
     */
    let link;

    beforeEach(() => {
        $http = sinon.stub();
        link = new SemanticLink($http, log);
    });

    describe('GET', () => {
        it('should match on /self/ returning a resolved promise', () => {

            let resource = {
                links: [
                    {rel: 'self', href: 'https://example.com'}
                ]
            };

            const response = {links: []};

            $http
                .withArgs(GET('https://example.com'))
                .returns(Promise.resolve(response));

            return link.get(resource, /self/)
                .then(result => {
                    expect(result.links).to.equal(response.links);
                    expect($http.called).to.be.true;

                });

        });

        it('should not match on /self/ when requesting /up/ returning a rejected promise', () => {

            let resource = {
                links: [
                    {
                        rel: 'up',
                        href: 'https://example.com'
                    }
                ]
            };

            $http
                .withArgs(GET('https://example.com'))
                .returns(Promise.resolve(''));

            return link.get(resource, /self/)
                .catch(err => {
                    expect($http.called).to.be.false;
                    expect(err).to.equal('The resource doesn\'t support the required interface');

                });

        });

    });

});