import {Representation} from "./interfaces";

/**
 * A guard to detect whether the object is a collection {@link Representation}
 *
 * @see https://stackoverflow.com/questions/14425568/interface-type-check-with-typescript
 * @param object
 * @returns whether the object is an instance on the interface
 * @private
 */
export function instanceOfCollection(object: any): object is Representation {
    return 'items' in object;
}
