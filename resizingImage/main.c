#include <stdio.h>
#define STB_IMAGE_IMPLEMENTATION
#include "stb_image.h"
#define STB_IMAGE_RESIZE_IMPLEMENTATION
#include "stb_image_resize2.h"
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "stb_image_write.h"

int main() {
    int width, height;
    int comp;
    stbi_uc * image= stbi_load("image.png", &width, &height, &comp, 0);

    printf("Size(%d, %d) %d\n", width, height, comp);

    char buffer[100] = {0};
    int sizes[] = {
        96,
        176,
        200,
        256,
        284,
        300,
        310,
        500,
    };
    size_t sizes_count = _countof(sizes);

    for (int i = 0; i < sizes_count; i++) {
        int osize = sizes[i];

        stbi_uc * oimage = stbir_resize_uint8_srgb(
            image, width, height, 0,
            NULL, osize, osize, 0,
            STBIR_RGBA
        );
        if (oimage == NULL) {
            printf("IMAGE COULDN'T BE RESIZED\n");
            break;
        }

        sprintf(buffer, "%dx%d.png", osize, osize);
        stbi_write_png(buffer, osize, osize, comp, oimage, 0);
        free(oimage);
    }

    stbi_image_free(image);
    return 0;
}

