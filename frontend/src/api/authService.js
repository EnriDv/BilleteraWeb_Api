import apiClient from "./apiClient.js";

export const AuthServices = {
  simulateApiCall: (data, success = true, delay = 1000) => {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        if (success) {
          resolve({ success: true, message: "Operación exitosa", data });
        } else {
          reject({ success: false, message: "Ocurrió un error", data });
        }
      }, delay);
    });
  },
  registerUser : (input) => {
    const registrationData = {
      fullName: input.fullName,
      phoneNumber: input.phoneNumber,
      email: input.email,
      pin: input.pin,
      otpCode: input.otpCode,
    };
    console.log(registrationData);
    return apiClient.post('/auth/register', registrationData);
  },
  login : (input) => {
    const loginData = {
      phoneNumber: input.phoneNumber,
      pin: input.pin,
    };
    return apiClient.post('/auth/login', loginData);
  },
  requestOtp : (input) => {
    const requestData = {
      phoneNumber: input.phoneNumber,
      email: input.email
    }
    return apiClient.post('/auth/request-otp', requestData);
  },
  ValidateOtp : (input) => {
    const requestData = {
      phoneNumber: input.phoneNumber,
      otpCode: input.otpCode
    }
    return apiClient.post('/auth/validate-otp', requestData);
  },
  verifyLiveness : (phone) => {
    console.log(`API Sim: Realizando liveness check para ${phone}`);
    const isSuccess = Math.random() > 0.3;
    if (!isSuccess) {
        return AuthServices.simulateApiCall({ liveness_verified: false, error: "LIVENESS_FAILED" });
    }
    return AuthServices.simulateApiCall({ liveness_verified: true});
  },
  requestPinRecoveryOtp : (input) => {
    const requestData = {
      phoneNumber: input.phoneNumber,
    };
    return apiClient.post('/auth/forgot-pin/request-otp', requestData);
  },
  resetPin : (input) => {
    const resetData = {
      phoneNumber: input.phoneNumber,
      otpCode: input.otpCode,
      newPin: input.newPin,
    };
    return apiClient.post('/auth/forgot-pin/reset', resetData);
  }
};

