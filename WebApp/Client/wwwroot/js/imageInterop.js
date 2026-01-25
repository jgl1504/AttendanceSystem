// imageInterop.js

window.createImageElement = (imageData) => {
    const img = new Image();
    img.src = imageData;
    document.body.appendChild(img);
};
