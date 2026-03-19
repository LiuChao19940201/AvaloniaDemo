export function getItem(key) {
    return localStorage.getItem(key);
}

export function setItem(key, value) {
    localStorage.setItem(key, value);
}

export function removeItem(key) {
    localStorage.removeItem(key);
}

// 允许的图片 MIME 前缀 和 常见图片扩展名（兜底）
const IMAGE_EXTENSIONS = [".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico", ".tiff", ".tif"];

function isImageFile(file) {
    // 优先检查 MIME type（大多数浏览器都会提供）
    if (file.type && file.type.startsWith("image/")) return true;
    // 兜底：部分浏览器对某些文件不设置 type，按扩展名判断
    const name = file.name.toLowerCase();
    return IMAGE_EXTENSIONS.some(ext => name.endsWith(ext));
}

export async function pickImageFile() {
    return new Promise((resolve) => {
        const input = document.createElement("input");
        input.type = "file";
        // ✅ 不设置 accept —— 避免浏览器全盘筛选导致卡死
        input.style.display = "none";
        document.body.appendChild(input);

        input.addEventListener("change", async () => {
            if (input.files && input.files.length > 0) {
                const file = input.files[0];

                // ✅ 选中后才校验是否为图片文件
                if (!isImageFile(file)) {
                    alert("请选择图片文件（支持 jpg、png、gif、bmp、webp 等格式）");
                    resolve(null);
                    document.body.removeChild(input);
                    return;
                }

                const buffer = await file.arrayBuffer();
                const bytes = new Uint8Array(buffer);
                let binary = "";
                const chunkSize = 8192;
                for (let i = 0; i < bytes.length; i += chunkSize) {
                    binary += String.fromCharCode(...bytes.subarray(i, i + chunkSize));
                }
                resolve(btoa(binary));
            } else {
                resolve(null);
            }
            document.body.removeChild(input);
        });

        input.addEventListener("cancel", () => {
            resolve(null);
            document.body.removeChild(input);
        });

        input.click();
    });
}