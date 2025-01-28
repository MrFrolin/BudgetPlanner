export function get() {
    return document.cookie;
}

export function set(key, value) {
    document.cookie = `${key}=${value}`;
}

//TODO: fungerar inte=??
export function remove(key) {
    document.cookie = `${key}=; expires=Thu, 01 Jan 1970 00:00:00 UTC;`;
}

