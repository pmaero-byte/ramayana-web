// Minimal Three.js stub for unit tests. Anything that needs real
// rendering stays in the Playwright smoke (tests/smoke). We just
// need stable class identity for `instanceof THREE.Group` checks
// in production modules that are imported into tests.

const stubClass = function () {};

function makeVec3(x = 0, y = 0, z = 0) {
  return {
    x, y, z,
    clone() { return makeVec3(this.x, this.y, this.z); },
    set(x, y, z) { this.x = x; this.y = y; this.z = z; return this; },
    copy(o) { this.x = o.x; this.y = o.y; this.z = o.z; return this; },
    addScaledVector(o, s) { this.x += o.x * s; this.y += o.y * s; this.z += o.z * s; return this; },
    multiplyScalar(s) { this.x *= s; this.y *= s; this.z *= s; return this; },
    normalize() {
      const l = Math.hypot(this.x, this.y, this.z) || 1;
      this.x /= l; this.y /= l; this.z /= l; return this;
    },
    applyQuaternion() { return this; },
    distanceTo(o) { return Math.hypot(this.x - o.x, this.y - o.y, this.z - o.z); },
  };
}
function makeVec2(x = 0, y = 0) {
  return { x, y, clone() { return makeVec2(this.x, this.y); }, set(x, y) { this.x = x; this.y = y; return this; } };
}
function makeEuler(x = 0, y = 0, z = 0, order = 'XYZ') {
  return {
    x, y, z, order,
    set(x, y, z, order) { this.x = x; this.y = y; this.z = z; this.order = order || this.order; return this; },
    copy(o) { this.x = o.x; this.y = o.y; this.z = o.z; this.order = o.order; return this; },
    clone() { return makeEuler(this.x, this.y, this.z, this.order); },
  };
}

class Group {
  constructor() {
    this.children = [];
    this.position = makeVec3(0, 0, 0);
    this.rotation = makeEuler(0, 0, 0, 'YXZ');
    this.quaternion = { setFromEuler() { return this; }, setFromUnitVectors() { return this; } };
    this.scale = { x: 1, y: 1, z: 1, set(x, y, z) { this.x = x; this.y = y; this.z = z; return this; }, setScalar(s) { this.x = s; this.y = s; this.z = s; return this; }, copy(o) { this.x = o.x; this.y = o.y; this.z = o.z; return this; } };
    this.visible = true;
    this.userData = {};
    this.renderOrder = 0;
  }
  add(...c) { this.children.push(...c); }
  remove(...c) { this.children = this.children.filter(x => !c.includes(x)); }
  traverse(fn) { fn(this); this.children.forEach(c => c.traverse && c.traverse(fn)); }
  clone() { return new Group(); }
}
class Object3D extends Group {}
class Points extends Object3D {
  constructor(geo, mat) { super(); this.geometry = geo; this.material = mat; this.isSprite = false; }
}
class Sprite extends Object3D {
  constructor(mat) { super(); this.material = mat; this.isSprite = true; }
}
class Mesh extends Object3D {
  constructor(geo, mat) { super(); this.geometry = geo; this.material = mat; this.castShadow = false; }
}
class PointsMaterial { constructor(opts) { Object.assign(this, opts || {}); } }
class SpriteMaterial { constructor(opts) { Object.assign(this, opts || {}); } }
class MeshStandardMaterial { constructor(opts) { Object.assign(this, opts || {}); } }
class MeshBasicMaterial { constructor(opts) { Object.assign(this, opts || {}); } }
class PlaneGeometry { dispose() {} }
class SphereGeometry { dispose() {} }
class CapsuleGeometry { dispose() {} }
class BoxGeometry { dispose() {} }
class ConeGeometry { dispose() {} }
class CylinderGeometry { dispose() {} }
class CircleGeometry { dispose() {} }
class RingGeometry { dispose() {} }
class TorusGeometry { dispose() {} }
class BufferGeometry {
  constructor() { this.attributes = {}; }
  setAttribute(name, attr) { this.attributes[name] = attr; }
  dispose() {}
}
class BufferAttribute {
  constructor(arr, size) { this.array = arr; this.itemSize = size; this.count = arr.length / size; this.needsUpdate = false; }
}
class Line extends Object3D {
  constructor(geo, mat) { super(); this.geometry = geo; this.material = mat; }
}
class LineBasicMaterial { constructor(opts) { Object.assign(this, opts || {}); this.opacity = (opts && opts.opacity) || 1; } }
class CanvasTexture { constructor(c) { this.image = c; this.needsUpdate = false; } dispose() {} }
class Color {
  constructor(v) { this.r = 1; this.g = 1; this.b = 1; }
  setHex(h) { return this; }
  setRGB(r, g, b) { this.r = r; this.g = g; this.b = b; return this; }
  setScalar() {}
  getHex() { return 0xffffff; }
  convertSRGBToLinear() { return this; }
}
class Vector2 { constructor(x = 0, y = 0) { this.x = x; this.y = y; } clone() { return new Vector2(this.x, this.y); } set() { return this; } }
class Vector3 {
  constructor(x = 0, y = 0, z = 0) {
    return Object.assign(makeVec3(x, y, z), { clone: function() { return makeVec3(this.x, this.y, this.z); } });
  }
}
class Quaternion { setFromEuler() { return this; } setFromUnitVectors() { return this; } }
class Euler {
  constructor(x = 0, y = 0, z = 0, o = 'XYZ') {
    return Object.assign(makeEuler(x, y, z, o), { clone: function() { return makeEuler(this.x, this.y, this.z, this.order); } });
  }
}
class Ray {
  constructor(origin, dir) { this.origin = origin || new Vector3(); this.direction = dir || new Vector3(0, 0, 1); }
  set(origin, dir) { this.origin = origin; this.direction = dir; }
  setFromCamera() { return this; }
  intersectObject() { return []; }
  intersectObjects(objs) { return objs || []; }
}
const MathUtils = {
  damp(a, b, k, dt) { return a + (b - a) * Math.min(1, k * dt); },
  clamp(v, lo, hi) { return Math.max(lo, Math.min(hi, v)); },
};

const DoubleSide = 2;
const AdditiveBlending = 5;
const SRGBColorSpace = 'srgb';
const LinearSRGBColorSpace = 'linear';

export {
  Group, Object3D, Points, Sprite, Mesh, PointsMaterial, SpriteMaterial,
  MeshStandardMaterial, MeshBasicMaterial, PlaneGeometry, SphereGeometry,
  CapsuleGeometry, BoxGeometry, ConeGeometry, CylinderGeometry, CircleGeometry,
  RingGeometry, TorusGeometry, BufferGeometry, BufferAttribute, Line, LineBasicMaterial,
  CanvasTexture, Color, Vector2, Vector3, Quaternion, Euler, Ray, MathUtils,
  DoubleSide, AdditiveBlending, SRGBColorSpace, LinearSRGBColorSpace,
};
